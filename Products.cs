using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EarningsPattern2
{
    delegate void ProductUpgradingEventHandler(Product Product);
    delegate void ProductClosingEventHandler(Product Product);

    enum ProductKind { Free, Fashionable, Classic, Large }

    [Serializable]
    class Product : MonthlyVariedObject
    {
        //public float Price { get; set; }
        //public float UpgradePrice { get; set; }
        public int ID;
        public bool Closed;
        public static float RateOfExchange = 6.8f;
        private static float bestVolumn = 20.00f * 30;
        public float Price
        {
            get
            {
                switch (this.Kind)
                {
                    case ProductKind.Free: return 0;
                    case ProductKind.Fashionable: return 30.00f;
                    case ProductKind.Classic: return 30.00f;
                    case ProductKind.Large: return 50.00f;
                    default: throw new InvalidOperationException();
                }
            }
        }
        public float UpgradePrice { get { return (float)(Price * 0.5); } }
        private int version;
        public int Version
        {
            get { return version; }
            set { version = value; iMonthLastReleased = MonthID; }
        }
        private int iMonthLastReleased = 0;
        public ProductKind Kind { get; set; }
        public event ProductUpgradingEventHandler ProductUpgrading;
        public event ProductClosingEventHandler ProductClosing;
        float[,] SalesVolumnTable = new float[,] { {0.00f,   0.00f,  0.00f,  0.00f,  0.00f,  0.00f},
                                                   {1.00f,   1.50f,  0.50f,  0.00f,  0.00f,  0.00f},
                                                   {0.50f,   1.00f,  1.00f,  1.00f,  0.50f,  0.00f},
                                                   {0.50f,   1.00f,  1.00f,  1.00f,  0.50f,  0.00f} };
        public override void NextMonth()
        {
            if ((Version == 5) && (iMonthLastReleased > 5)) ProductClosing(this);
            if (iMonthLastReleased + 6 < MonthID)
            {
                if (Kind == ProductKind.Free && Version == 4 || Kind != ProductKind.Free && SalesVolumnTable[(int)Kind, Version + 1] == 0)
                {
                    ProductClosing(this);
                }
                else
                {
                    ProductUpgrading(this);
                }
            }
        }
        float GetNewSalesVolumn() { return SalesVolumnTable[(int)Kind, Version - 1] * bestVolumn; }
        float GetUpgradeSalesVolumn() { return SalesVolumnTable[(int)Kind, Version - 1 - 1] * bestVolumn; }
        float GetSales()
        {
            // in USD
            float sales = Price * GetNewSalesVolumn();
            if (Version > 1) sales += UpgradePrice * GetUpgradeSalesVolumn();
            return (sales);
        }
        float GetNetSales()
        {
            // in USD
            return (float)(GetSales() * 0.98);
        }
        public float GetProductRevene()
        {
            // in RMB, agent fee deducted
            return (float)(GetNetSales() * 0.9 * RateOfExchange);
        }
        float[,] AdvertisingFeeTable = new float[,] { {0.25f,   0.00f,  0.00f,  0.00f,  0.00f},
                                                      {1.50f,   1.00f,  0.00f,  0.00f,  0.00f},
                                                      {1.25f,   1.00f,  1.00f,  0.50f,  0.00f},
                                                      {1.50f,   1.00f,  1.00f,  1.00f,  0.50f} };
        public float GetAdvertisingFee()
        {
            const float baseAdvertisingFee = 2000.00f; // in USD
            return baseAdvertisingFee * AdvertisingFeeTable[(int)Kind, Version] * RateOfExchange; // in RMB
        }

    }

    [Serializable]
    class ProductList : List<Product>, IProfitable, IIndebted
    {
        private float GetTotalProductRevenue()
        {
            return this.Sum(c => c.GetProductRevene());
        }
        private float GetCrossSellingYield()
        {
            return this.Count == 0 ? 1 : (float)(1 + Math.Log10(this.Count));
        }
        public float GetMonthRevenue()
        {
            return GetTotalProductRevenue() * GetCrossSellingYield();
        }

        #region IIndebted Members

        public float GetMonthCost()
        {
            // Advertising fee
            return this.Sum(p => p.GetAdvertisingFee());
        }

        #endregion
    }

}
