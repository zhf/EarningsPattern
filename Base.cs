using System;

namespace EarningsPattern2
{
    interface IIndebted
    {
        float GetMonthCost();
    }

    interface IProfitable
    {
        float GetMonthRevenue();
    }

    interface IPhysical
    {
        float GetRequiredSpace(); // in SqM;
        float GetRequiredPower(); // in 1000 Watt/Hour
    }

    [Serializable]
    abstract class MonthlyVariedObject
    {
        public MonthlyVariedObject()
        {
        }

        public MonthlyVariedObject(int monthID)
        {
            this.iMonth = monthID;
        }
        public int iMonth = -1;
        public int MonthID
        {
            get
            {
                return iMonth;
            }
            set
            {
                if (iMonth > value)
                {
                    throw new Exception("invalid iMonth value");
                }
                if (iMonth < value)
                {
                    iMonth = value;
                    NextMonth();
                }
                else
                {
                    iMonth = value;
                }
            }
        }
        public abstract void NextMonth();

    }

    class MyException : ApplicationException
    {
    }
    class NoStaffException : MyException
    {
    }
    class InsufficientFundsException : MyException
    {
    }
    class SafeModeException : MyException
    {
    }
    class TooManyLargeProjectsException : MyException
    {
    }
    class TooEarlyOrTooBusyForFreeThings : MyException
    {
    }

    //delegate bool CanCreateProjectEventHandler(Product OriginalProduct);
    //delegate void ProjectCreatedEventHandler(Project Project);
    //delegate void ProjectClosed(Project Project);
    delegate void MonthlyReportEventHandler(Report r);


}