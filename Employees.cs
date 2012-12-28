using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EarningsPattern2
{
    //enum StaffType { Recruiter, Developer, Researcher, ProgramManager, ProductManager, MarketAnalyst, Tester, Artist };
    [Serializable]
    abstract class Staff : MonthlyVariedObject
    {
        public Staff(float Skill, float nMonthsOfExperience, float Scope)
        {
            this.Skill = Skill;
            this.nMonthsOfExperience = nMonthsOfExperience;
            this.Scope = Scope;
            nMonthsInCompany = 0;
        }

        public Staff()
        {
            this.Skill = 5;
            this.nMonthsOfExperience = 60;
            this.Scope = 3;
            nMonthsInCompany = 0;
        }

        protected virtual float BaseSalary { get { return 3000.00f; } }
        protected virtual float ConcurrencyCoefficient { get { return 1.0f; } }
        protected float Skill { get; set; }
        protected float nMonthsOfExperience { get; set; }
        protected float Scope { get; set; }
        protected float nMonthsToRecruit { get; set; }
        protected float CostToRecruit { get; set; }
        protected int nMonthsInCompany;
        public Staff Mentor { get; set; }
        public bool CanBeMentor { get { return nMonthsInCompany > 6; } }
        public float Salary { get { return GetSalary(); } }
        private int _nParticipatedProjects;

        protected virtual float GetSalary()
        {
            return (float)(BaseSalary * (0 + Math.Log10(Skill) + Math.Log10(nMonthsOfExperience / 12) + Math.Log10(Scope)));
        }

        public virtual float GetRequiredSpace()
        {
            return 4.0f;
        }

        public virtual float GetPowerUsed()
        {
            return 2 * 25; // 1000W/H * Business Day per Month
        }

        private float GetConcurrencyCapability()
        {
            //return (float)((Math.Log10(Skill) + Math.Log10(nMonthsOfExperience / 12) + 1) * ConcurrencyCoefficient);
            return ConcurrencyCoefficient;
            //return 1;
        }

        public int nParticipatedProjects
        {
            get
            {
                return _nParticipatedProjects;
            }
            set
            {
                if (value > GetConcurrencyCapability()) throw new Exception();
                _nParticipatedProjects = value;
            }
        }

        public bool CanParticipateInMoreProjects()
        {
            return GetConcurrencyCapability() - _nParticipatedProjects >= 1;
        }

        public override void NextMonth()
        {
            nMonthsInCompany++;
            nMonthsOfExperience++;
            if (nMonthsInCompany >= 3) Mentor = null;
        }
    }

    [Serializable]
    class Programmer : Staff
    {
        protected override float BaseSalary { get { return 5000.00f; } }
        public override float GetRequiredSpace()
        {
            return 9.0f;
        }
        //public Programmer(float Skill, float MonthOfExperience, float Scope)
        //    : base(Skill, MonthOfExperience, Scope)
        //{
        //    BaseSalary = 5000.00f;
        //}
    }

    [Serializable]
    class Researcher : Programmer
    {
        protected override float ConcurrencyCoefficient { get { return 2.0f; } }
    }

    [Serializable]
    class Developer : Programmer
    {
        protected override float BaseSalary { get { return 6000.00f; } }
        //public Developer(float Skill, float MonthOfExperience, float Scope)
        //    : base(Skill, MonthOfExperience, Scope)
        //{
        //    BaseSalary = 5000.00f;
        //}

        bool IsSenior { get; set; }
    }

    [Serializable]
    class ProgramManager : Programmer
    {
        protected override float BaseSalary { get { return 8000.00f; } }
        protected override float ConcurrencyCoefficient { get { return 2.0f; } }
        //public ProgramManager(float Skill, float MonthOfExperience, float Scope)
        //    : base(Skill, MonthOfExperience, Scope)
        //{
        //    BaseSalary = 6000.00f;
        //    ConcurrencyCoefficient = 2.0f;
        //}
    }

    [Serializable]
    class ProductManager : Staff
    {
        protected override float BaseSalary { get { return 8000.00f; } }
        protected override float ConcurrencyCoefficient { get { return 4.0f; } }
    }

    [Serializable]
    class Tester : Staff
    {
        protected override float BaseSalary { get { return 4000.00f; } }
    }

    [Serializable]
    class Artist : Staff
    {
        protected override float BaseSalary { get { return 3000.00f; } }
        protected override float ConcurrencyCoefficient { get { return 4.0f; } }
    }

    [Serializable]
    class Recruiter : Staff
    {
        protected override float BaseSalary { get { return 8000.00f; } }
        //public Recruiter(float Skill, float MonthOfExperience, float Scope)
        //    : base(Skill, MonthOfExperience, Scope)
        //{
        //    BaseSalary = 3000.00f;
        //    //MyScheduler = new List<RecruitSchedulerItem>();
        //}
        public float GetCapability() // To Recruit Num of Employees Per Month()
        {
            return (float)(Math.Log10(Skill) + Math.Log10(nMonthsOfExperience / 12));
        }
        //List<RecruitSchedulerItem> MyScheduler;
        public float GetQuota()
        {
            //return GetCapability() - MyScheduler.Count;
            throw new NotImplementedException();
        }
    }

    [Serializable]
    class CEO : Staff
    {
        protected override float BaseSalary { get { return 10000.00f; } }
        public CEO()
        {
            nMonthsInCompany = 12;
        }
        public override float GetRequiredSpace()
        {
            return 12.0f;
        }

    }

    [Serializable]
    class TechSupport : Staff
    {
        protected override float BaseSalary { get { return 2500.00f; } }
    }

    [Serializable]
    class DocumentManager : Staff
    {
        protected override float BaseSalary { get { return 2500.00f; } }
        protected override float ConcurrencyCoefficient { get { return 3.0f; } }
    }

    [Serializable]
    class StaffList : List<Staff>, IIndebted, IPhysical
    {
        public float GetMonthCost()
        {
            return this.Sum(c => c.Salary);
        }

        public float GetRequiredSpace()
        {
            return this.Sum(c => c.GetRequiredSpace());
        }

        public float GetRequiredPower()
        {
            return this.Sum(c => c.GetPowerUsed());
        }
    }
}
