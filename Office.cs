using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EarningsPattern2
{
    [Serializable]
    class Office : MonthlyVariedObject, IIndebted
    {
        public Office()
        {
            PhysicalObjectsInOffice = new List<IPhysical>();
        }
        static float OfficeRentalRate = 3; // Per Square Meter
        public float TotalSpaceInSqM { get; set; } // SQ.M.
        static float PowerRate = 1; // RMB per 1000W/h
        float TotalPowerConsumed;
        //float SQMPerDeveloper;
        public List<IPhysical> PhysicalObjectsInOffice;
        public float GetMonthCost()
        {
            return OfficeRentalRate * TotalSpaceInSqM + PowerRate * TotalPowerConsumed;
        }
        public override void NextMonth()
        {
            TotalSpaceInSqM = PhysicalObjectsInOffice.Sum(c => c.GetRequiredSpace());
            TotalPowerConsumed = PhysicalObjectsInOffice.Sum(c => c.GetRequiredPower());
        }
    }

}
