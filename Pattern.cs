using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace EarningsPattern2
{
    public struct Report
    {
        public int MonthID;
        public int nEmployees;
        public int ProductKind;
        public int nIdle;
        public int nProjects;
        public int nProducts;
        public float MonthlyCost;
        public float MonthlyRevenue;
        public float Balance;
    }


    enum Mode { Growing, Limited };
    
    class Pattern
    {
        public Pattern(int nMonthsToRun)
        {
            this.nMonthsToRun = nMonthsToRun;
            //SafeHistory = new Stack<Company>();
            //GrowHistory = new Stack<Company>();
            
        }

        private int nMonthsToRun;
        public event MonthlyReportEventHandler OnMonthlyReport;
        //int[] AI;
        //int GetPredefindedResults(int iMonth)
        //{
        //    AI = new int[37];
        //    AI[0] = 1;
        //    AI[1] = 0;
        //    AI[2] = 0;
        //    AI[3] = 0;
        //    AI[4] = 0;
        //    AI[5] = 0;
        //    AI[6] = 1;
        //    AI[7] = 1;
        //    AI[8] = 0;
        //    AI[9] = 0;
        //    AI[10] = 1;
        //    AI[11] = 0;
        //    AI[12] = 1;
        //    AI[13] = 0;
        //    AI[14] = 1;
        //    AI[15] = 0;
        //    AI[16] = 1;
        //    AI[17] = 1;
        //    AI[18] = 0;
        //    AI[19] = 0;
        //    AI[20] = 1;
        //    AI[21] = 1;
        //    AI[22] = 0;
        //    AI[23] = 1;
        //    AI[24] = 1;
        //    AI[25] = 1;
        //    AI[26] = 1;
        //    AI[27] = 1;
        //    AI[28] = 1;
        //    AI[29] = 2;
        //    AI[30] = 1;
        //    AI[31] = 1;
        //    AI[32] = 1;
        //    AI[33] = 2;
        //    AI[34] = 1;
        //    AI[35] = 2;
        //    AI[36] = 2;

        //    //switch (iMonth)
        //    //{
        //    //    case 0: return 1;
        //    //    case 1: return 0;
        //    //    case 2: return 0;
        //    //    case 3: return 0;
        //    //    case 4: return 0;
        //    //    case 5: return 0;
        //    //    case 6: return 0;
        //    //    case 7: return 0;
        //    //    case 8: return 0;
        //    //    default: return -1;
        //    //}
        //    //if (iMonth <= 9) return AI[iMonth]; else return -1;
        //    return AI[iMonth];
        //}

        public int[] currentDecisions;
        //struct Result
        //{
        //    public Result(int cMonths, float balance)
        //    {
        //        Decisions = new ProductKind[cMonths];
        //        Balance = balance;
        //    }
        //    public float Balance;
        //    public ProductKind[] Decisions;
        //    public MemoryStream[] CompanyStates;
        //}

        List<int[]> decisionsList;

        void RecordCurrentDecisions()
        {
            decisionsList.Add((int[])currentDecisions.Clone());
        }

        int GetSuggestion(int iMonth)
        {
            switch (iMonth)
            {
                case 0: 
                case 1:
                case 2:
                case 3:
                case 4:
                    return -1;
                default: return -2;
            }
        }

        void TryNextMonth2(Company company, int kind)
        {
            if (decisionsList.Count >= 1) return;

            //var cs = CloneCompanyState(company);
            int i = company.MonthID;
            i++;
            Debug.WriteLine(String.Format("-------------------- MonthID = {0:D}, {1:S}", i, "Real Mode"));
            company.iMonth = i;
            if (kind > -1) company.CreateNewProduct((ProductKind)kind);

            currentDecisions[i] = kind;
            try
            {
                company.NextMonth();
            }
            catch (MyException)
            {
                //result.Balance = 0;
                //iResult = -1;
                return;
            }

            if (i >= nMonthsToRun - 1)
            {
                RecordCurrentDecisions();
                //var result = new Result(nMonthsToRun, cs.Accounting.Balance);
                //result.Balance = cs.Accounting.Balance;
                //result.Balance = company.Accounting.Balance;
                //resultList.Add(result);
                return;
            }
            else
            {
                var snapshot = CompanySaveState(company);
                
                for (int k = (int)ProductKind.Large; k >= -1; k--)
                {
                    if (k == (int)ProductKind.Classic || k == (int)ProductKind.Free) continue;
                    // Using suggestions
                    int suggestion = GetSuggestion(i);
                    if (suggestion >= -1 && suggestion != k) continue;
                    // Do not create new product at the end of the test period
                    if (nMonthsToRun - i < 6) k = -1;

                    Company cs;
                    if (company == null)
                    {
                        cs = CompanyRestoreState(snapshot);
                    }
                    else
                    {
                        cs = company;
                        company = null;
                    }
                    TryNextMonth2(cs, k);
                }
            }
        }

        public void Run2()
        {
            Company company = new Company();
            company.SafeMode = false;

            decisionsList = new List<int[]>();
            currentDecisions = new int[nMonthsToRun];
            TryNextMonth2(company, 1);

            
            var stateSequenceList = new List<Company[]>();
            int iMaxBalance = 0;
            float maxBalance = 0;
            for (int j = 0; j < decisionsList.Count; j++)
            {
                var d = decisionsList[j];
                Company[] stateSequence = new Company[nMonthsToRun];
                Company c = new Company();
                for (int i = 0; i < nMonthsToRun; i++)
                {
                    c.iMonth = i;
                    c.CreateNewProduct((ProductKind)d[i]);
                    c.NextMonth();
                    stateSequence[i] = CompanyRestoreState(CompanySaveState(c));
                }
                if (c.Accounting.Balance > maxBalance)
                {
                    maxBalance = c.Accounting.Balance;
                    iMaxBalance = j;
                }
                stateSequenceList.Add(stateSequence);
            }
            for (int i = 0; i < nMonthsToRun; i++)
            {
                NotifyMonthDone(i, (int)decisionsList[iMaxBalance][i], stateSequenceList[iMaxBalance][i]);
            }

        }

        //public void Run()
        //{
        //    //GrowHistory.Clear();
        //    Company company = new Company();
        //    company.SafeMode = false;
        //    //GrowHistory.Push(company);
        //    //TryGrowNextMonth();
            
        //    for (int i = 0; i <= nMonthsToRun; i++)
        //    {
        //        Debug.WriteLine(String.Format("-------------------- MonthID = {0:D}, {1:S}", i, "Grow Mode"));
        //        company.MonthID = i; // This state would not raise error since it's been test-driven or it's pre-defined
        //        Company lastGoodState = CloneCompanyState(company);
        //        int n = GetPredefindedResults(i);
        //        if (n < 0)
        //        {
        //            n = 0;
        //            bool successfullyDrived;
        //            var snapshot = CloneCompanyState(company);
        //            do
        //            {
        //                //lastState = CloneCompanyState(company);
        //                if (n != 0)
        //                {
        //                    company.CreateNewProducts(ProductKind.Classic);
        //                }
        //                successfullyDrived = TestDrive(company);
        //                if (successfullyDrived)
        //                {
        //                    lastGoodState = CloneCompanyState(company);
        //                    company = CloneCompanyState(snapshot); // Reset the state
        //                    n++;
        //                }
        //            } while (successfullyDrived);
        //            n--;
        //            company = CloneCompanyState(lastGoodState);
        //            if (n < 0) throw new Exception("Bad model");
        //        }
        //        else
        //        {
        //            for (int j = 0; j < n; j++)
        //            {
        //                if (j == 1)
        //                {
        //                    company.CreateNewProducts(ProductKind.Classic);
        //                }
        //                lastGoodState = CloneCompanyState(company);
        //            }
        //        }
        //        NotifyMonthDone(i, n, lastGoodState);
        //    }            
        //}

        //bool TestDrive(Company company)
        //{
        //    Company cs = CloneCompanyState(company);
        //    //if (!company.SafeMode)
        //    //{
        //    //    SafeHistory.Clear();
        //    //    SafeHistory.Push(CloneCompanyState(company));
        //    //}
        //    //cs = SafeHistory.Peek();
        //    cs.SafeMode = true;
        //    return TestNextMonth(cs) == TestResult.Okay;
        //}

        //enum TestResult { Okay, SafeMode, InsufficientFunds, NoStaff }
        //TestResult TestNextMonth(Company cs)
        //{
        //    //Company cs = SafeHistory.Peek();
        //    int i = cs.MonthID;
        //    i++;
        //    Debug.WriteLine(String.Format("-------------------- MonthID = {0:D}, {1:S}", i, "Safe Mode"));

        //    try
        //    {
        //        cs.MonthID = i;
        //        if (i >= nMonthsToRun)
        //        {
        //            Debug.WriteLine("Test ended successfully.");
        //            return TestResult.Okay;
        //        }
        //    }
        //    catch (MyException ex)
        //    {
        //        //SafeHistory.Pop();
        //        if (ex is SafeModeException) return TestResult.SafeMode;
        //        else
        //            if (ex is InsufficientFundsException) return TestResult.InsufficientFunds;
        //            else
        //                if (ex is NoStaffException) return TestResult.NoStaff;
        //                else
        //                    throw;
        //    }

        //    if (i <= nMonthsToRun - 1)
        //    {
        //        TestResult testResult;
        //        int n = 0;
        //        var snapshot = CloneCompanyState(cs);
                
        //        //Company lastState;
        //        TestResult lastResult = TestResult.Okay;
        //        do
        //        {
        //            //lastState = CloneCompanyState(cs);
        //            //SafeHistory.Push(CloneCompanyState(cs));
        //            if (n == 1)
        //                cs.CreateNewProducts(ProductKind.Classic);
        //            testResult = TestNextMonth(cs);
        //            switch (testResult)
        //            {
        //                case TestResult.Okay:
        //                    {
        //                        return TestResult.Okay;
        //                    }
        //                case TestResult.InsufficientFunds: 
        //                    {
        //                        if (lastResult == TestResult.SafeMode)
        //                        {
        //                            return TestResult.InsufficientFunds;
        //                        }
        //                        else
        //                        {
        //                            cs = CloneCompanyState(snapshot);
        //                            n++;
        //                            lastResult = TestResult.InsufficientFunds;
        //                            break;
        //                        }
        //                    }
        //                case TestResult.SafeMode:
        //                    {
        //                        if (lastResult == TestResult.InsufficientFunds)
        //                        {
        //                            return TestResult.SafeMode;
        //                        }
        //                        else
        //                        {
        //                            cs = CloneCompanyState(snapshot);
        //                            n--;
        //                            lastResult = TestResult.SafeMode;
        //                            if (n < 0) return TestResult.SafeMode;
        //                            break;
        //                        }
        //                    }
        //                case TestResult.NoStaff:
        //                    {
        //                        return TestResult.NoStaff;
        //                    }
        //                default:
        //                    {
        //                        Debug.Assert(false);
        //                        break;
        //                    }
        //            }

        //        } while (true);
        //        //n--;

        //    }
        //    else return TestNextMonth(cs);
        //}

        //public override void NextMonth()
        //{
        //    Debug.WriteLine("--------------------");
        //    Debug.WriteLine("MonthID = " + MonthID.ToString());
        //    Debug.WriteLine("Plan to create " + actions[MonthID].ToString() + " project(s).");
        //    if (actions[MonthID] < 0) throw new ApplicationException("action failed.");
        //    projectCreatedThisMonth = 0;
        //    Employees.MonthID = MonthID;
        //    BusinessCycles.MonthID = MonthID;
        //    Accounting.MonthID = MonthID;
        //    Debug.WriteLine("Totally " + projectCreatedThisMonth.ToString() + " project(s) created this month.");
        //    Debug.WriteLine(String.Format("nEmployees = {0:D}, nProjects = {1:D}, nProducts = {2:D}", Employees.GetNumOfEmployees(), BusinessCycles.NumberOfProjects, BusinessCycles.NumberOfProducts));
        //}

        void NotifyMonthDone(int iMonth, int ProductKind, Company companyState)
        {
            Report r = new Report();
            r.MonthID = iMonth;
            r.nEmployees = companyState.NumberOfEmployees;
            r.nIdle = companyState.NumberOfIdleStaffs;
            r.ProductKind = ProductKind;
            r.nProjects = companyState.NumberOfProjects;
            r.nProducts = companyState.NumberOfProducts;
            r.MonthlyCost = companyState.Accounting.MonthlyCost / 10000;
            r.MonthlyRevenue = companyState.Accounting.MonthlyRevenue / 10000;
            r.Balance = companyState.Accounting.Balance / 10000;
            OnMonthlyReport(r);
        }

        //Company CloneCompanyState(Company company)
        //{
        //    using (var memStream = new MemoryStream())
        //    {
        //        var binaryFormatter = new BinaryFormatter();
        //        binaryFormatter.Serialize(memStream, company);
        //        memStream.Seek(0, SeekOrigin.Begin);
        //        return (Company)binaryFormatter.Deserialize(memStream);
        //    }
        //}

        MemoryStream CompanySaveState(Company company)
        {
            var memStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memStream, company);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        Company CompanyRestoreState(MemoryStream state)
        {
            var binaryFormatter = new BinaryFormatter();
            state.Seek(0, SeekOrigin.Begin);
            return (Company)binaryFormatter.Deserialize(state);
        }
        
        //int TryCreateNewProducts(ref Company companyState, int iMonth)
        //{
        //    int n = 0;
        //    //bool breakedBySafeException = false;
        //    Company snapshot;
        //    while (true)
        //    {
        //        Debug.WriteLine("* Backup Company State");
        //        snapshot = CloneCompanyState(companyState);
        //        try
        //        {
        //            companyState.CreateOneNewProduct();
        //            companyState.SafeMode = true;
        //            TryNextMonth(ref companyState, iMonth + 1);
        //            n++;
        //        }
        //        catch (MyException e)
        //        {
        //            Debug.WriteLine("* Restore Company State");
        //            companyState = CloneCompanyState(snapshot);
        //            //breakedBySafeException = e is SafeModeException;
        //            break;
        //        }
        //    }
        //    companyState.SafeMode = false;
        //    return n;
        //}

        //Stack<Company> GrowHistory;
        //Stack<Company> SafeHistory;

        //Company CreateOneNewProduct()
        //{
        //    var companyState = GrowHistory.Peek();
        //    Debug.WriteLine("* Backup Company State");
        //    Company newCompanyState = CloneCompanyState(companyState);
        //    GrowHistory.Push(newCompanyState);
        //    newCompanyState.CreateOneNewProduct();
        //    return newCompanyState;
        //}

        //bool TrySafeNextMonth()
        //{
        //    var companyState = SafeHistory.Peek();
        //    try
        //    {
        //        int i = companyState.MonthID + 1;
        //        if (i > nMonthsToRun) return true;
        //        Debug.WriteLine(String.Format("-------------------- MonthID = {0:D}, {1:S}", i, "Safe Mode"));
        //        companyState.MonthID = i;
        //    }
        //    catch (MyException)
        //    {
        //        if (SafeHistory.Count == 1)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            SafeHistory.Pop();
        //            return TrySafeNextMonth();
        //        }
        //    }
        //    // Try safely create new products without recruitement
        //    while (true)
        //    {
        //        // Backup state before creating each new product
        //        var newState = CloneCompanyState(companyState);
        //        SafeHistory.Push(newState);
        //        // Now modify the new state
        //        newState.CreateOneNewProduct();
        //        // Test the new state
        //        if (TrySafeNextMonth())
        //        {
        //            //SafeHistory.Pop();
        //            return true;
        //        }
        //        else
        //        {
        //            // Undo creating the last project
        //            //SafeHistory.Pop();
        //            return false;
        //        }
        //    }
        //}

        //bool TryGrowNextMonth()
        //{
        //    var companyState = GrowHistory.Peek();

        //    try
        //    {
        //        int i = companyState.MonthID + 1;
        //        if (i >= nMonthsToRun) return true;
        //        Debug.WriteLine(String.Format("-------------------- MonthID = {0:D}, {1:S}", i, "Grow Mode"));
        //        companyState.MonthID = i;
        //    }
        //    catch (MyException)
        //    {
        //        GrowHistory.Pop();
        //        return false;
        //    }


        //    int n = 0;
        //    try
        //    {
        //        while (true)
        //        {
        //            if (companyState.MonthID < 9)
        //            {
        //                if (companyState.MonthID == 0)
        //                {
        //                    companyState = CreateOneNewProduct();
        //                    n = 1;
        //                }
        //                break;
        //            }
        //            else
        //            {
        //                companyState = CreateOneNewProduct();
        //            }

        //            //if (!companyState.SafeMode)
        //            //{
        //            //    Debug.WriteLine("Entering SafeMode");
        //            //    companyState.SafeMode = true;
        //            //}
        //            companyState.SafeMode = true;
        //            SafeHistory.Clear();
        //            SafeHistory.Push(companyState);
        //            if (TrySafeNextMonth())
        //            {
        //                n++;
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //        if (!companyState.SafeMode)
        //        {
        //            NotifyMonthDone(companyState.MonthID, n, companyState);
        //            Debug.WriteLine(String.Format("===== Finish calc month #{0:D}, okay to start {1:D} product(s). =====", companyState.MonthID, n));
        //        }
        //        return TryGrowNextMonth();
        //    }
        //    catch (MyException ex)
        //    {
        //        //Debug.WriteLine("Handling Exception, Type is " + ex.GetType().ToString());
        //        companyState = GrowHistory.Pop();
        //        Debug.WriteLine("* Company State has been restored, now MonthID = " + companyState.MonthID.ToString());
        //        return false;
        //    }

        //    //TryNextMonth(ref companyState);
            
        //}

    }
}
