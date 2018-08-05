using System;
using IfcEngineCS;
using _3DModel.ViewModel;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace _3DModel.IFC
{
    public class IFCTreeData
    {
        IfcEngine IFCEngine;
        IntPtr IfcModel;
        IFCItem IfcRoot;
        TreeView treeControl;

        Dictionary<string, bool> checkedElementsDictionary = new Dictionary<string, bool>();

        public string ModelName { get; set; }

        public IFCTreeData(IfcEngine engine, IntPtr model, IFCItem item, TreeView treeviewControl)
        {
            IFCEngine = engine;
            IfcModel = model;
            IfcRoot = item;
            treeControl = treeviewControl;
        }

        public TreeView BuildTree()
        {
            if (IfcModel == IntPtr.Zero)
            {
                throw new ArgumentException("Invalid model.");
            }

            if (IfcRoot == null)
            {
                throw new ArgumentException("The root is null.");
            }

            checkedElementsDictionary.Clear();

            CreateHeaderTreeItems();
            CreateProjectTreeItems();
            CreateNotReferencedTreeItems();

            return treeControl;
        }

        private void CreateHeaderTreeItems()
        {
            var tnHeaderInfo = new TreeViewItem() { Header = "Header Info" };
            treeControl.Items.Add(tnHeaderInfo);
            // Descriptions
            var tnDescriptions = new TreeViewItem() { Header = "Descriptions" };
            tnHeaderInfo.Items.Add(tnDescriptions);

            int counter = 0;
            while (IFCEngine.GetSPFFHeaderItem(IfcModel, 0, counter++, IfcEngine.SdaiType.Unicode, out IntPtr description) == IntPtr.Zero)
            {
                var tnDescription = new TreeViewItem() { Header = Marshal.PtrToStringUni(description) };
                tnDescriptions.Items.Add(tnDescription);
            }

            // ImplementationLevel
            IFCEngine.GetSPFFHeaderItem(IfcModel, 1, 0, IfcEngine.SdaiType.Unicode, out IntPtr implementationLevel);

            var tnImplementationLevel = new TreeViewItem() { Header = "ImplementationLevel = '" + Marshal.PtrToStringUni(implementationLevel) + "'" };
            tnHeaderInfo.Items.Add(tnImplementationLevel);

            // Name
            IFCEngine.GetSPFFHeaderItem(IfcModel, 2, 0, IfcEngine.SdaiType.Unicode, out IntPtr name);

            this.ModelName = Marshal.PtrToStringUni(name);
            var tnName = new TreeViewItem() { Header = "Name = '" + Marshal.PtrToStringUni(name) + "'" };
            tnHeaderInfo.Items.Add(tnName);

            // TimeStamp
            IFCEngine.GetSPFFHeaderItem(IfcModel, 3, 0, IfcEngine.SdaiType.Unicode, out IntPtr timeStamp);

            var tnTimeStamp = new TreeViewItem() { Header = "TimeStamp = '" + Marshal.PtrToStringUni(timeStamp) + "'" };
            tnHeaderInfo.Items.Add(tnTimeStamp);

            // Authors
            var tnAuthors = new TreeViewItem() { Header = "Authors" };
            tnHeaderInfo.Items.Add(tnAuthors);

            counter = 0;
            while (IFCEngine.GetSPFFHeaderItem(IfcModel, 4, counter++, IfcEngine.SdaiType.Unicode, out IntPtr author) == IntPtr.Zero)
            {
                var tnAuthor = new TreeViewItem() { Header = Marshal.PtrToStringUni(author) };
                tnAuthors.Items.Add(tnAuthor);
            }

            // Organizations
            var tnOrganizations = new TreeViewItem() { Header = "Organizations" };
            tnHeaderInfo.Items.Add(tnOrganizations);

            counter = 0;
            while (IFCEngine.GetSPFFHeaderItem(IfcModel, 5, counter++, IfcEngine.SdaiType.Unicode, out IntPtr organization) == IntPtr.Zero)
            {
                var tnOrganization = new TreeViewItem() { Header = Marshal.PtrToStringUni(organization) };
                tnOrganizations.Items.Add(tnOrganization);
            }

            // PreprocessorVersion
            IFCEngine.GetSPFFHeaderItem(IfcModel, 6, 0, IfcEngine.SdaiType.Unicode, out IntPtr preprocessorVersion);

            var tnPreprocessorVersion = new TreeViewItem() { Header = "PreprocessorVersion = '" + Marshal.PtrToStringUni(preprocessorVersion) + "'" };
            tnHeaderInfo.Items.Add(tnPreprocessorVersion);

            // OriginatingSystem
            IFCEngine.GetSPFFHeaderItem(IfcModel, 7, 0, IfcEngine.SdaiType.Unicode, out IntPtr originatingSystem);

            var tnOriginatingSystem = new TreeViewItem() { Header = "OriginatingSystem = '" + Marshal.PtrToStringUni(originatingSystem) + "'" };
            tnHeaderInfo.Items.Add(tnOriginatingSystem);

            // Authorization
            IFCEngine.GetSPFFHeaderItem(IfcModel, 8, 0, IfcEngine.SdaiType.Unicode, out IntPtr authorization);

            var tnAuthorization = new TreeViewItem() { Header = "Authorization = '" + Marshal.PtrToStringUni(authorization) + "'" };
            tnHeaderInfo.Items.Add(tnAuthorization);

            // FileSchemas
            var tnFileSchemas = new TreeViewItem() { Header = "FileSchemas" };
            tnHeaderInfo.Items.Add(tnFileSchemas);

            counter = 0;
            while (IFCEngine.GetSPFFHeaderItem(IfcModel, 9, counter++, IfcEngine.SdaiType.Unicode, out IntPtr fileSchema) == IntPtr.Zero)
            {
                var tnFileSchema = new TreeViewItem() { Header = Marshal.PtrToStringUni(fileSchema) };
                tnFileSchemas.Items.Add(tnFileSchema);
            }
        }

        private void CreateProjectTreeItems()
        {
            var iEntityID = IFCEngine.GetEntityExtent(IfcModel, "IfcProject");
            var iEntitiesCount = IFCEngine.GetMemberCount(iEntityID);

            for (int iEntity = 0; iEntity < iEntitiesCount.ToInt32(); iEntity++)
            {
                IFCEngine.GetAggregationElement(iEntityID, iEntity, IfcEngine.SdaiType.Instance, out IntPtr iInstance);

                IFCTreeItem ifcTreeItem = new IFCTreeItem();
                ifcTreeItem.instance = iInstance;

                CreateTreeItem(null, ifcTreeItem);

                AddChildrenTreeItems(ifcTreeItem, iInstance, "IfcSite");
            }
        }

        private void CreateNotReferencedTreeItems()
        {
            var ifcTreeItem = new IFCTreeItem();
            ifcTreeItem.treeNode = new TreeViewItem() { Header = "Not Referenced" };
            treeControl.Items.Add(ifcTreeItem.treeNode);
            ifcTreeItem.treeNode.Foreground = new SolidColorBrush(Colors.Gray);
            ifcTreeItem.treeNode.Tag = ifcTreeItem;

            FindNonReferencedIFCItems(IfcRoot, ifcTreeItem.treeNode);

            if (ifcTreeItem.treeNode.Items.Count == 0)
            {
                // don't show empty Not Referenced item
                treeControl.Items.Remove(ifcTreeItem.treeNode);
            }
        }

        private void CreateTreeItem(IFCTreeItem ifcParent, IFCTreeItem ifcItem)
        {
            IntPtr ifcType = IFCEngine.GetInstanceType(ifcItem.instance);
            string strIfcType = Marshal.PtrToStringAnsi(ifcType);

            IFCEngine.GetAttribute(ifcItem.instance, "Name", IfcEngine.SdaiType.Unicode, out IntPtr name);

            string strName = Marshal.PtrToStringUni(name);

            IFCEngine.GetAttribute(ifcItem.instance, "Description", IfcEngine.SdaiType.Unicode, out IntPtr description);

            string strDescription = Marshal.PtrToStringUni(description);

            string strItemText = "'" + (string.IsNullOrEmpty(strName) ? "<name>" : strName) +
                    "', '" + (string.IsNullOrEmpty(strDescription) ? "<description>" : strDescription) +
                    "' (" + strIfcType + ")";

            if ((ifcParent != null) && (ifcParent.treeNode != null))
            {
                ifcItem.treeNode = new TreeViewItem() { Header = strItemText };
                ifcParent.treeNode.Items.Add(ifcItem.treeNode);
            }
            else
            {
                ifcItem.treeNode = new TreeViewItem() { Header = strItemText };
                treeControl.Items.Add(ifcItem.treeNode);
            }

            if (ifcItem.ifcItem == null)
            {
                // item without visual representation
                Random rand = new Random();
                byte[] colorValue = new byte[4];
                rand.NextBytes(colorValue);

                var color = System.Windows.Media.Color.FromArgb(
                    (byte)(255 - colorValue[0] * 255),
                    (byte)(colorValue[1] * 255),
                    (byte)(colorValue[2] * 255),
                    (byte)(colorValue[3] * 255));


                ifcItem.treeNode.Foreground = new SolidColorBrush(color);//Colors.Gray
            }

            ifcItem.treeNode.Tag = ifcItem;
        }

        private void AddChildrenTreeItems(IFCTreeItem ifcParent, IntPtr iParentInstance, string strEntityName)
        {
            // check for decomposition
            IFCEngine.GetAttribute(iParentInstance, "IsDecomposedBy", IfcEngine.SdaiType.Aggregation, out IntPtr decompositionInstance);

            if (decompositionInstance == IntPtr.Zero)
            {
                return;
            }

            var iDecompositionsCount = IFCEngine.GetMemberCount(decompositionInstance);
            for (int iDecomposition = 0; iDecomposition < iDecompositionsCount.ToInt32(); iDecomposition++)
            {
                IFCEngine.GetAggregationElement(decompositionInstance, iDecomposition, IfcEngine.SdaiType.Instance, out IntPtr iDecompositionInstance);

                if (!IsInstanceOf(iDecompositionInstance, "IFCRELAGGREGATES"))
                {
                    continue;
                }

                IFCEngine.GetAttribute(iDecompositionInstance, "RelatedObjects", IfcEngine.SdaiType.Aggregation, out IntPtr objectInstances);

                var iObjectsCount = IFCEngine.GetMemberCount(objectInstances);
                for (int iObject = 0; iObject < iObjectsCount.ToInt32(); iObject++)
                {
                    IntPtr iObjectInstance = IntPtr.Zero;
                    IFCEngine.GetAggregationElement(objectInstances, iObject, IfcEngine.SdaiType.Instance, out iObjectInstance);

                    if (!IsInstanceOf(iObjectInstance, strEntityName))
                    {
                        continue;
                    }

                    IFCTreeItem ifcTreeItem = new IFCTreeItem();
                    ifcTreeItem.instance = iObjectInstance;

                    CreateTreeItem(ifcParent, ifcTreeItem);

                    switch (strEntityName)
                    {
                        case "IfcSite":
                            {
                                AddChildrenTreeItems(ifcTreeItem, iObjectInstance, "IfcBuilding");
                            }
                            break;

                        case "IfcBuilding":
                            {
                                AddChildrenTreeItems(ifcTreeItem, iObjectInstance, "IfcBuildingStorey");
                            }
                            break;

                        case "IfcBuildingStorey":
                            {
                                AddElementTreeItems(ifcTreeItem, iObjectInstance);
                            }
                            break;

                        default:
                            break;
                    }
                } // for (int iObject = ...
            } // for (int iDecomposition = ...
        }

        private void AddElementTreeItems(IFCTreeItem ifcParent, IntPtr iParentInstance)
        {
            IFCEngine.GetAttribute(iParentInstance, "IsDecomposedBy", IfcEngine.SdaiType.Aggregation, out IntPtr decompositionInstance);

            if (decompositionInstance == IntPtr.Zero)
            {
                return;
            }

            var iDecompositionsCount = IFCEngine.GetMemberCount(decompositionInstance);
            for (int iDecomposition = 0; iDecomposition < iDecompositionsCount.ToInt32(); iDecomposition++)
            {
                IFCEngine.GetAggregationElement(decompositionInstance, iDecomposition, IfcEngine.SdaiType.Instance, out IntPtr iDecompositionInstance);

                if (!IsInstanceOf(iDecompositionInstance, "IFCRELAGGREGATES"))
                {
                    continue;
                }

                IFCEngine.GetAttribute(iDecompositionInstance, "RelatedObjects", IfcEngine.SdaiType.Aggregation, out IntPtr objectInstances);

                var iObjectsCount = IFCEngine.GetMemberCount(objectInstances);
                for (int iObject = 0; iObject < iObjectsCount.ToInt32(); iObject++)
                {
                    IFCEngine.GetAggregationElement(objectInstances, iObject, IfcEngine.SdaiType.Instance, out IntPtr iObjectInstance);

                    IFCTreeItem ifcTreeItem = new IFCTreeItem();
                    ifcTreeItem.instance = iObjectInstance;
                    ifcTreeItem.ifcItem = FindIFCItem(IfcRoot, ifcTreeItem);

                    CreateTreeItem(ifcParent, ifcTreeItem);

                    checkedElementsDictionary[GetItemType(iObjectInstance)] = true;

                    if (ifcTreeItem.ifcItem != null)
                    {
                        ifcTreeItem.ifcItem.ifcTreeItem = ifcTreeItem;
                    }
                } // for (int iObject = ...
            } // for (int iDecomposition = ...

            // check for elements
            IFCEngine.GetAttribute(iParentInstance, "ContainsElements", IfcEngine.SdaiType.Aggregation, out IntPtr elementsInstance);

            if (elementsInstance == IntPtr.Zero)
            {
                return;
            }

            var iElementsCount = IFCEngine.GetMemberCount(elementsInstance);
            for (int iElement = 0; iElement < iElementsCount.ToInt32(); iElement++)
            {
                IFCEngine.GetAggregationElement(elementsInstance, iElement, IfcEngine.SdaiType.Instance, out IntPtr iElementInstance);

                if (!IsInstanceOf(iElementInstance, "IFCRELCONTAINEDINSPATIALSTRUCTURE"))
                {
                    continue;
                }

                IFCEngine.GetAttribute(iElementInstance, "RelatedElements", IfcEngine.SdaiType.Aggregation, out IntPtr objectInstances);

                var iObjectsCount = IFCEngine.GetMemberCount(objectInstances);
                for (int iObject = 0; iObject < iObjectsCount.ToInt32(); iObject++)
                {
                    var iObjectInstance = IntPtr.Zero;
                    IFCEngine.GetAggregationElement(objectInstances, iObject, IfcEngine.SdaiType.Instance, out iObjectInstance);

                    IFCTreeItem ifcTreeItem = new IFCTreeItem();
                    ifcTreeItem.instance = iObjectInstance;
                    ifcTreeItem.ifcItem = FindIFCItem(IfcRoot, ifcTreeItem);

                    CreateTreeItem(ifcParent, ifcTreeItem);

                    checkedElementsDictionary[GetItemType(iObjectInstance)] = true;

                    if (ifcTreeItem.ifcItem != null)
                    {
                        ifcTreeItem.ifcItem.ifcTreeItem = ifcTreeItem;

                        GetColor(ifcTreeItem);
                    }

                    IFCEngine.GetAttribute(iObjectInstance, "IsDefinedBy", IfcEngine.SdaiType.Aggregation, out IntPtr definedByInstances);

                    if (definedByInstances == IntPtr.Zero)
                    {
                        continue;
                    }

                    var iDefinedByCount = IFCEngine.GetMemberCount(definedByInstances);
                    for (int iDefinedBy = 0; iDefinedBy < iDefinedByCount.ToInt32(); iDefinedBy++)
                    {
                        IFCEngine.GetAggregationElement(definedByInstances, iDefinedBy, IfcEngine.SdaiType.Instance, out IntPtr iDefinedByInstance);

                        if (IsInstanceOf(iDefinedByInstance, "IFCRELDEFINESBYPROPERTIES"))
                        {
                            AddPropertyTreeItems(ifcTreeItem, iDefinedByInstance);
                        }
                        else
                        {
                            if (IsInstanceOf(iDefinedByInstance, "IFCRELDEFINESBYTYPE"))
                            {
                                // NA
                            }
                        }
                    }
                } // for (int iObject = ...
            } // for (int iDecomposition = ...
        }

        private bool IsInstanceOf(IntPtr iInstance, string strType)
        {
            if (IFCEngine.GetInstanceType(iInstance) == IFCEngine.GetEntity(IfcModel, strType))
            {
                return true;
            }

            return false;
        }

        void GetColor(IFCTreeItem ifcTreeItem)
        {
            if (ifcTreeItem == null)
            {
                throw new ArgumentException("The item is null.");
            }

            // C++ => getRGB_object()
            IFCEngine.GetAttribute(ifcTreeItem.instance, "Representation", IfcEngine.SdaiType.Instance, out IntPtr representationInstance);
            if (representationInstance == IntPtr.Zero)
            {
                return;
            }

            // C++ => getRGB_productDefinitionShape()
            IFCEngine.GetAttribute(representationInstance, "Representations", IfcEngine.SdaiType.Aggregation, out IntPtr representationsInstance);

            var iRepresentationsCount = IFCEngine.GetMemberCount(representationsInstance);
            for (int iRepresentation = 0; iRepresentation < iRepresentationsCount.ToInt32(); iRepresentation++)
            {
                IFCEngine.GetAggregationElement(representationsInstance, iRepresentation, IfcEngine.SdaiType.Instance, out IntPtr iShapeInstance);

                if (iShapeInstance == IntPtr.Zero)
                {
                    continue;
                }

                // C++ => getRGB_shapeRepresentation()
                IFCEngine.GetAttribute(iShapeInstance, "RepresentationIdentifier", IfcEngine.SdaiType.Unicode, out IntPtr representationIdentifier);

                if (Marshal.PtrToStringUni(representationIdentifier) == "Body")
                {
                    IFCEngine.GetAttribute(iShapeInstance, "Items", IfcEngine.SdaiType.Aggregation, out IntPtr itemsInstance);

                    var iItemsCount = IFCEngine.GetMemberCount(itemsInstance);
                    for (int iItem = 0; iItem < iItemsCount.ToInt32(); iItem++)
                    {
                        IFCEngine.GetAggregationElement(itemsInstance, iItem, IfcEngine.SdaiType.Instance, out IntPtr iItemInstance);

                        IFCEngine.GetAttribute(iItemInstance, "StyledByItem", IfcEngine.SdaiType.Instance, out IntPtr styledByItem);

                        if (styledByItem != IntPtr.Zero)
                        {
                            GetRGB_styledItem(ifcTreeItem, styledByItem);
                        }
                        else
                        {
                            SearchDeeper(ifcTreeItem, iItemInstance);
                        } // else if (iItemInstance != 0)

                        if (ifcTreeItem.ifcColor != null)
                        {
                            return;
                        }
                    } // for (int iItem = ...
                }
            } // for (int iRepresentation = ...
        }

        void SearchDeeper(IFCTreeItem ifcTreeItem, IntPtr iParentInstance)
        {
            IFCEngine.GetAttribute(iParentInstance, "StyledByItem", IfcEngine.SdaiType.Instance, out IntPtr styledByItem);

            if (styledByItem != IntPtr.Zero)
            {
                GetRGB_styledItem(ifcTreeItem, styledByItem);
                if (ifcTreeItem.ifcColor != null)
                {
                    return;
                }
            }

            if (IsInstanceOf(iParentInstance, "IFCBOOLEANCLIPPINGRESULT"))
            {
                IFCEngine.GetAttribute(iParentInstance, "FirstOperand", IfcEngine.SdaiType.Instance, out IntPtr firstOperand);

                if (firstOperand != IntPtr.Zero)
                {
                    SearchDeeper(ifcTreeItem, firstOperand);
                }
            } // if (IsInstanceOf(iParentInstance, "IFCBOOLEANCLIPPINGRESULT"))
            else
            {
                if (IsInstanceOf(iParentInstance, "IFCMAPPEDITEM"))
                {
                    IFCEngine.GetAttribute(iParentInstance, "MappingSource", IfcEngine.SdaiType.Instance, out IntPtr mappingSource);

                    IFCEngine.GetAttribute(mappingSource, "MappedRepresentation", IfcEngine.SdaiType.Instance, out IntPtr mappedRepresentation);

                    if (mappedRepresentation != IntPtr.Zero)
                    {
                        IFCEngine.GetAttribute(mappedRepresentation, "RepresentationIdentifier", IfcEngine.SdaiType.Unicode, out IntPtr representationIdentifier);

                        if (Marshal.PtrToStringUni(representationIdentifier) == "Body")
                        {
                            styledByItem = IntPtr.Zero;

                            IFCEngine.GetAttribute(mappedRepresentation, "Items", IfcEngine.SdaiType.Aggregation, out IntPtr itemsInstance);

                            var iItemsCount = IFCEngine.GetMemberCount(itemsInstance);
                            for (int iItem = 0; iItem < iItemsCount.ToInt32(); iItem++)
                            {
                                IFCEngine.GetAggregationElement(itemsInstance, iItem, IfcEngine.SdaiType.Instance, out IntPtr iItemInstance);

                                IFCEngine.GetAttribute(iItemInstance, "StyledByItem", IfcEngine.SdaiType.Instance, out styledByItem);

                                if (styledByItem != IntPtr.Zero)
                                {
                                    GetRGB_styledItem(ifcTreeItem, styledByItem);
                                }
                                else
                                {
                                    SearchDeeper(ifcTreeItem, iItemInstance);
                                } // else if (iItemInstance != 0)

                                if (ifcTreeItem.ifcColor != null)
                                {
                                    return;
                                }
                            } // for (int iItem = ...
                        } // if (Marshal.PtrToStringAnsi(representationIdentifier) == "Body")
                    } // if (mappedRepresentation != IntPtr.Zero)
                } // if (IsInstanceOf(iParentInstance, "IFCMAPPEDITEM"))
            } // else if (IsInstanceOf(iParentInstance, "IFCBOOLEANCLIPPINGRESULT"))
        }

        void GetRGB_styledItem(IFCTreeItem ifcTreeItem, IntPtr iStyledByItemInstance)
        {
            IFCEngine.GetAttribute(iStyledByItemInstance, "Styles", IfcEngine.SdaiType.Aggregation, out IntPtr stylesInstance);

            var iStylesCount = IFCEngine.GetMemberCount(stylesInstance);
            for (int iStyle = 0; iStyle < iStylesCount.ToInt32(); iStyle++)
            {
                IFCEngine.GetAggregationElement(stylesInstance, iStyle, IfcEngine.SdaiType.Instance, out IntPtr iStyleInstance);

                if (iStyleInstance == IntPtr.Zero)
                {
                    continue;
                }

                GetRGB_presentationStyleAssignment(ifcTreeItem, iStyleInstance);
            } // for (int iStyle = ...
        }

        void GetRGB_presentationStyleAssignment(IFCTreeItem ifcTreeItem, IntPtr iParentInstance)
        {
            IntPtr stylesInstance;
            IFCEngine.GetAttribute(iParentInstance, "Styles", IfcEngine.SdaiType.Aggregation, out stylesInstance);

            var iStylesCount = IFCEngine.GetMemberCount(stylesInstance);
            for (int iStyle = 0; iStyle < iStylesCount.ToInt32(); iStyle++)
            {
                var iStyleInstance = IntPtr.Zero;
                IFCEngine.GetAggregationElement(stylesInstance, iStyle, IfcEngine.SdaiType.Instance, out iStyleInstance);

                if (iStyleInstance == IntPtr.Zero)
                {
                    continue;
                }

                GetRGB_surfaceStyle(ifcTreeItem, iStyleInstance);
            } // for (int iStyle = ...
        }

        unsafe void GetRGB_surfaceStyle(IFCTreeItem ifcTreeItem, IntPtr iParentInstance)
        {
            IFCEngine.GetAttribute(iParentInstance, "Styles", IfcEngine.SdaiType.Aggregation, out IntPtr stylesInstance);

            var iStylesCount = IFCEngine.GetMemberCount(stylesInstance);
            for (int iStyle = 0; iStyle < iStylesCount.ToInt32(); iStyle++)
            {
                IFCEngine.GetAggregationElement(stylesInstance, iStyle, IfcEngine.SdaiType.Instance, out IntPtr iStyleInstance);

                if (iStyleInstance == IntPtr.Zero)
                {
                    continue;
                }

                IFCEngine.GetAttribute(iStyleInstance, "SurfaceColour", IfcEngine.SdaiType.Instance, out IntPtr surfaceColour);

                if (surfaceColour == IntPtr.Zero)
                {
                    continue;
                }

                double R = 0;
                IFCEngine.GetAttribute(surfaceColour, "Red", IfcEngine.SdaiType.Real, out *(IntPtr*)&R);

                double G = 0;
                IFCEngine.GetAttribute(surfaceColour, "Green", IfcEngine.SdaiType.Real, out *(IntPtr*)&G);

                double B = 0;
                IFCEngine.GetAttribute(surfaceColour, "Blue", IfcEngine.SdaiType.Real, out *(IntPtr*)&B);

                ifcTreeItem.ifcColor = new IFCItemColor();
                ifcTreeItem.ifcColor.R = (float)R;
                ifcTreeItem.ifcColor.G = (float)G;
                ifcTreeItem.ifcColor.B = (float)B;

                return;
            } // for (int iStyle = ...
        }

        private IFCItem FindIFCItem(IFCItem ifcParent, IFCTreeItem ifcTreeItem)
        {
            if (ifcParent == null)
            {
                return null;
            }

            IFCItem ifcIterator = ifcParent;
            while (ifcIterator != null)
            {
                if (ifcIterator.ifcID == ifcTreeItem.instance)
                {
                    return ifcIterator;
                }

                IFCItem ifcItem = FindIFCItem(ifcIterator.child, ifcTreeItem);
                if (ifcItem != null)
                {
                    return ifcItem;
                }

                ifcIterator = ifcIterator.next;
            }

            return FindIFCItem(ifcParent.child, ifcTreeItem);
        }

        private string GetItemType(IntPtr iInstance)
        {
            IntPtr ifcType = IFCEngine.GetInstanceType(iInstance);
            return Marshal.PtrToStringAnsi(ifcType);
        }

        private void AddPropertyTreeItems(IFCTreeItem ifcParent, IntPtr iParentInstance)
        {
            IFCEngine.GetAttribute(iParentInstance, "RelatingPropertyDefinition", IfcEngine.SdaiType.Instance, out IntPtr propertyInstances);

            if (IsInstanceOf(propertyInstances, "IFCELEMENTQUANTITY"))
            {
                IFCTreeItem ifcPropertySetTreeItem = new IFCTreeItem();
                ifcPropertySetTreeItem.instance = propertyInstances;

                CreateTreeItem(ifcParent, ifcPropertySetTreeItem);

                // check for quantity
                IFCEngine.GetAttribute(propertyInstances, "Quantities", IfcEngine.SdaiType.Aggregation, out IntPtr quantitiesInstance);

                if (quantitiesInstance == IntPtr.Zero)
                {
                    return;
                }

                var iQuantitiesCount = IFCEngine.GetMemberCount(quantitiesInstance);
                for (int iQuantity = 0; iQuantity < iQuantitiesCount.ToInt32(); iQuantity++)
                {
                    IFCEngine.GetAggregationElement(quantitiesInstance, iQuantity, IfcEngine.SdaiType.Instance, out IntPtr iQuantityInstance);

                    IFCTreeItem ifcQuantityTreeItem = new IFCTreeItem();
                    ifcQuantityTreeItem.instance = iQuantityInstance;

                    if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYLENGTH"))
                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcQuantityTreeItem, "IFCQUANTITYLENGTH");
                    else
                        if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYAREA"))
                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcQuantityTreeItem, "IFCQUANTITYAREA");
                    else
                            if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYVOLUME"))
                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcQuantityTreeItem, "IFCQUANTITYVOLUME");
                    else
                                if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYCOUNT"))
                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcQuantityTreeItem, "IFCQUANTITYCOUNT");
                    else
                                    if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYWEIGTH"))
                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcQuantityTreeItem, "IFCQUANTITYWEIGTH");
                    else
                                        if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYTIME"))
                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcQuantityTreeItem, "IFCQUANTITYTIME");
                } // for (int iQuantity = ...
            }
            else
            {
                if (IsInstanceOf(propertyInstances, "IFCPROPERTYSET"))
                {
                    IFCTreeItem ifcPropertySetTreeItem = new IFCTreeItem();
                    ifcPropertySetTreeItem.instance = propertyInstances;

                    CreateTreeItem(ifcParent, ifcPropertySetTreeItem);

                    // check for quantity
                    IFCEngine.GetAttribute(propertyInstances, "HasProperties", IfcEngine.SdaiType.Aggregation, out IntPtr propertiesInstance);

                    if (propertiesInstance == IntPtr.Zero)
                    {
                        return;
                    }

                    var iPropertiesCount = IFCEngine.GetMemberCount(propertiesInstance);
                    for (int iProperty = 0; iProperty < iPropertiesCount.ToInt32(); iProperty++)
                    {
                        IFCEngine.GetAggregationElement(propertiesInstance, iProperty, IfcEngine.SdaiType.Instance, out IntPtr iPropertyInstance);

                        if (!IsInstanceOf(iPropertyInstance, "IFCPROPERTYSINGLEVALUE"))
                            continue;

                        IFCTreeItem ifcPropertyTreeItem = new IFCTreeItem();
                        ifcPropertyTreeItem.instance = iPropertyInstance;

                        CreatePropertyTreeItem(ifcPropertySetTreeItem, ifcPropertyTreeItem, "IFCPROPERTYSINGLEVALUE");
                    } // for (int iProperty = ...
                }
            }
        }

        private void CreatePropertyTreeItem(IFCTreeItem ifcParent, IFCTreeItem ifcItem, string strProperty)
        {
            IntPtr ifcType = IFCEngine.GetInstanceType(ifcItem.instance);
            string strIfcType = Marshal.PtrToStringAnsi(ifcType);

            IntPtr name;
            IFCEngine.GetAttribute(ifcItem.instance, "Name", IfcEngine.SdaiType.Unicode, out name);

            string strName = Marshal.PtrToStringUni(name);

            string strValue = string.Empty;
            switch (strProperty)
            {
                case "IFCQUANTITYLENGTH":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "LengthValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                case "IFCQUANTITYAREA":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "AreaValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                case "IFCQUANTITYVOLUME":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "VolumeValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                case "IFCQUANTITYCOUNT":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "CountValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                case "IFCQUANTITYWEIGTH":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "WeigthValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                case "IFCQUANTITYTIME":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "TimeValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                case "IFCPROPERTYSINGLEVALUE":
                    {
                        IFCEngine.GetAttribute(ifcItem.instance, "NominalValue", IfcEngine.SdaiType.Unicode, out IntPtr value);

                        strValue = Marshal.PtrToStringUni(value);
                    }
                    break;

                default:
                    throw new Exception("Unknown property.");
            } // switch (strProperty)    

            string strItemText = "'" + (string.IsNullOrEmpty(strName) ? "<name>" : strName) +
                    "' = '" + (string.IsNullOrEmpty(strValue) ? "<value>" : strValue) +
                    "' (" + strIfcType + ")";

            if ((ifcParent != null) && (ifcParent.treeNode != null))
            {
                ifcItem.treeNode = new TreeViewItem() { Header = strItemText };
                ifcParent.treeNode.Items.Add(ifcItem.treeNode);
            }
            else
            {
                ifcItem.treeNode = new TreeViewItem() { Header = strItemText };
                treeControl.Items.Add(ifcItem.treeNode);
            }

            if (ifcItem.ifcItem == null)
            {
                // item without visual representation
                ifcItem.treeNode.Foreground = new SolidColorBrush(Colors.Gray);
            }

        }

        private void FindNonReferencedIFCItems(IFCItem ifcParent, TreeViewItem tnNotReferenced)
        {
            if (ifcParent == null)
            {
                return;
            }

            IFCItem ifcIterator = ifcParent;
            while (ifcIterator != null)
            {
                if ((ifcIterator.ifcTreeItem == null) && (ifcIterator.ifcID != IntPtr.Zero))
                {
                    string strItemText = "'" + (string.IsNullOrEmpty(ifcIterator.name) ? "<name>" : ifcIterator.name) +
                            "' = '" + (string.IsNullOrEmpty(ifcIterator.description) ? "<description>" : ifcIterator.description) +
                            "' (" + (string.IsNullOrEmpty(ifcIterator.ifcType) ? ifcIterator.globalID : ifcIterator.ifcType) + ")";

                    IFCTreeItem ifcTreeItem = new IFCTreeItem();
                    ifcTreeItem.instance = ifcIterator.ifcID;
                    ifcTreeItem.treeNode = new TreeViewItem() { Header = strItemText };
                    tnNotReferenced.Items.Add(ifcTreeItem.treeNode);
                    ifcTreeItem.ifcItem = FindIFCItem(IfcRoot, ifcTreeItem);
                    ifcIterator.ifcTreeItem = ifcTreeItem;
                    ifcTreeItem.treeNode.Tag = ifcTreeItem;

                    if (ifcTreeItem.ifcItem != null)
                    {
                        ifcTreeItem.ifcItem.ifcTreeItem = ifcTreeItem;
                    }
                }

                FindNonReferencedIFCItems(ifcIterator.child, tnNotReferenced);

                ifcIterator = ifcIterator.next;
            }

            FindNonReferencedIFCItems(ifcParent.child, tnNotReferenced);
        }


    }
}
