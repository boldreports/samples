using System.Collections.Generic;

namespace JSONtoWebAPIViewerSample.Models
{
    public class ProductList
    {
        public string LogDate { get; set; }
        public string DEPARTMENT { get; set; }
        public string ProdGroup { get; set; }
        public string MONO { get; set; }
        public string MOCOLOR { get; set; }
        public string MOSIZE { get; set; }
        public string CODE { get; set; }
        public string SEQNO { get; set; }
        public string ColorSequence { get; set; }
        public string SIZESEQUENCE { get; set; }
        public string COLORDESCRIPTION { get; set; }
        public string QTY { get; set; }
    
  
        public static System.Collections.IList GetData()
        {
            List<ProductList> datas = new List<ProductList>();
            ProductList data = null;

            data = new ProductList()
            {
                LogDate = "2024-06-06",
                DEPARTMENT = "Electronics",
                ProdGroup = "Mobile Phones",
                MONO = "Samsung Galaxy S21",
                MOCOLOR = "Black",
                MOSIZE = "128GB",
                CODE = "SGS21-BLK-128",
                SEQNO = "001",
                ColorSequence = "1",
                SIZESEQUENCE = "1",
                COLORDESCRIPTION = "Phantom Black",
                QTY = "150"
            };
            datas.Add(data);


            data = new ProductList()
            {
                LogDate = "2024-06-07",
                DEPARTMENT = "Home Appliances",
                ProdGroup = "Refrigerators",
                MONO = "LG Smart Fridge",
                MOCOLOR = "Silver",
                MOSIZE = "300L",
                CODE = "LG-SF-300",
                SEQNO = "002",
                ColorSequence = "1",
                SIZESEQUENCE = "2",
                COLORDESCRIPTION = "Platinum Silver",
                QTY = "75"
            };
            datas.Add(data);



            return datas;
        }
    }
}
