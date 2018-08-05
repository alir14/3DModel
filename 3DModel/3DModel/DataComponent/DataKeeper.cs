using _3DModel.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DModel.DataComponent
{
    public class DataKeeper
    {
        private static DataKeeper instance = null;

        private static object Key = new object();

        private DataKeeper()
        { }

        public static DataKeeper Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (Key)
                    {
                        if (instance == null)
                        {
                            instance = new DataKeeper();
                        }
                    }
                }

                return instance;
            }
        }

        List<ModelEntity> dataBase = new List<ModelEntity>();

        public List<ModelEntity> DataBase
        {
            get { return dataBase; }
        }


        public void Save(ModelEntity comment)
        {
            this.dataBase.Add(comment);
        }

        public ModelEntity ReadData(string modelName, string itemId)
        {
            return this.dataBase.FirstOrDefault<ModelEntity>(x => x.ModelName == modelName && x.SelectedItemId == itemId);
        }
    }
}
