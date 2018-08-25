using System;
using System.Linq;
using System.Text;
using _3DModel.ViewModel;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        List<DetailModel> dataBase = new List<DetailModel>();

        public List<DetailModel> DataBase
        {
            get { return dataBase; }
        }


        public void Save(DetailModel comment)
        {
            this.dataBase.Add(comment);
        }

        public DetailModel ReadData(string modelName, string itemId)
        {
            return this.dataBase.FirstOrDefault<DetailModel>(x => x.ModelName == modelName && x.SelectedItemId == itemId);
        }
    }
}
