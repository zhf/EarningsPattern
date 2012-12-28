using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EarningsPattern2
{
    delegate void GetAccountingItemsEventHandler();

    [Serializable]
    class Accounting : MonthlyVariedObject
    {
        public Accounting(float IntialBalance)
        {
            this.Balance = IntialBalance;
            IndebtedItems = new List<IIndebted>();
            ProfitableItems = new List<IProfitable>();
        }
        public float Balance;
        public float MonthlyCost;
        public float MonthlyRevenue;
        public List<IIndebted> IndebtedItems;
        public List<IProfitable> ProfitableItems;

        void Calc()
        {
            MonthlyCost = this.IndebtedItems.Sum(c => c.GetMonthCost());
            MonthlyRevenue = this.ProfitableItems.Sum(c => c.GetMonthRevenue());
            Balance += MonthlyRevenue;
            Balance -= MonthlyCost;
        }

        public override void NextMonth()
        {
            Calc();
            if (Balance < 0) throw new InsufficientFundsException();
        }

        //public bool IsBalanceLow { get { return Balance < 10 * 10000; } }


    }
}
