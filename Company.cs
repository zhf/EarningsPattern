using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EarningsPattern2
{
    //TODO: use Serialization
    [Serializable]
    class Company : MonthlyVariedObject
    {
        public Company()
        {
            AllProducts = new ProductList();// new List<Product>();
            AllProjects = new List<Project>();
            AllEmployees = new StaffList();
            CEO = new CEO();
            AllEmployees.Add(CEO);
            Accounting = new Accounting(1500000);
            Office = new Office();
            Office.PhysicalObjectsInOffice.Add(AllEmployees);
            //BusinessCycles.ProjectCreated += new ProjectCreatedEventHandler(BusinessCycles_ProjectCreated);
            Accounting.ProfitableItems.Add(AllProducts);
            Accounting.IndebtedItems.Add(AllEmployees);
            Accounting.IndebtedItems.Add(Office);
            Accounting.IndebtedItems.Add(AllProducts);
        }

        ProductList AllProducts;
        List<Project> AllProjects;
        StaffList AllEmployees;
        CEO CEO;
        public Accounting Accounting;
        Office Office;
        public int NumberOfProjects { get { return AllProjects.Count; } }
        public int NumberOfProducts { get { return AllProducts.Count; } }
        public int NumberOfEmployees { get { return AllEmployees.Count; } }
        public int NumberOfIdleStaffs { get { return AllEmployees.Count(s => s is Programmer && !AllProjects.Any(p => p.CurrentMembers.Contains(s))); } }
        public bool SafeMode;
        int lastProductID = -1;
        int lastProjectID = -1;

        Staff FindMentorForNewHire(Type staffType)
        {
            var ex = AllEmployees.Where(e => e.GetType().Equals(staffType));
            if (NumberOfProjects == 1) return CEO; // if no such type of staff, let the CEO be the mentor
            if (ex.Count() == 0 && NumberOfProducts == 1) return CEO; // hire tech support when releasing first product
            Staff staff = ex.FirstOrDefault(c => c.CanBeMentor && !AllEmployees.Any(d => d.Mentor == c));
            return staff;
        }

        public Staff RecruitStaff(Type staffType)
        {
            // Recruit only when any mentor is available
            Staff mentor = FindMentorForNewHire(staffType);
            if (mentor != null)
            {
                Staff staff = Activator.CreateInstance(staffType) as Staff;
                staff.Mentor = mentor;
                AllEmployees.Add(staff);
                Debug.WriteLine("Recruited a " + staffType.Name);
                return staff;
            }
            else
            {
                Debug.WriteLine("No mentor available on recruiting a " + staffType.Name);
                return null;
            }
        }

        void LaunchProduct(Project project)
        {
            Product p;
            if (project.OriginalProduct == null)
            {
                p = new Product();
                p.iMonth = MonthID;
                p.ID = ++lastProductID;
                p.ProductUpgrading += new ProductUpgradingEventHandler(OnProductUpgrading);
                p.ProductClosing += new ProductClosingEventHandler(OnProductClosing);
                p.Version = 1;
                p.Kind = project.TargetProductKind;
                p.Closed = false;
                AllProducts.Add(p);
                HireTechSupportStaffs();
            }
            else
            {
                p = project.OriginalProduct;
                p.Version = p.Version + 1;
            }
            Debug.WriteLine("Product launched! Version = " + p.Version.ToString() + "; ID = " + p.ID.ToString());
        }

        private void HireTechSupportStaffs()
        {
            int cTechSupportStaffs = AllEmployees.Count(c => c is TechSupport);
            // Try to recruit a tech support staff. No exception will be throw if nobody is recruited.
            if (cTechSupportStaffs < NumberOfProducts) RecruitStaff(typeof(TechSupport));
        }

        void OnProductClosing(Product Product)
        {
            Debug.WriteLine("Product Closing.");
            Product.Closed = true;
        }

        void OnProductUpgrading(Product Product)
        {
            if (!AllProjects.Any(c => c.OriginalProduct == Product))
            {
                Debug.WriteLine("Upgrading Product.");
                ProjectKind kind;
                switch (Product.Kind)
                {
                    case ProductKind.Free:
                        kind = ProjectKind.SmallUpgrade;
                        break;
                    case ProductKind.Fashionable:
                    case ProductKind.Classic:
                        kind = ProjectKind.MediumUpgrade;
                        break;
                    case ProductKind.Large:
                        kind = ProjectKind.LargeUpgrade;
                        break;
                    default: throw new InvalidOperationException();
                }
                CreateProject(kind, Product);
            }
        }

        public float GetMonthRevenue()
        {
            return AllProducts.Sum(c => c.GetProductRevene());
        }

        void CheckProjectDiversity()
        {
            if (AllProjects.Count(p => p.Kind == ProjectKind.Large) > NumberOfProjects * 0.2) throw new TooManyLargeProjectsException();
            if (NumberOfProducts > 3 && NumberOfProjects < 3) throw new TooEarlyOrTooBusyForFreeThings();
        }

        public void CreateNewProduct(ProductKind Kind)
        {
            if (Kind < 0) return;
            Project p;
            switch (Kind)
            {
                case ProductKind.Free:
                    p = CreateProject(ProjectKind.Small, null);
                    break;
                case ProductKind.Fashionable:
                case ProductKind.Classic:
                    p = CreateProject(ProjectKind.Medium, null);
                    break;
                case ProductKind.Large:
                    p = CreateProject(ProjectKind.Large, null);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            p.TargetProductKind = Kind;
            Debug.WriteLine("Creating a new " + Kind.ToString() + " product!");
        }

        Project CreateProject(ProjectKind Kind, Product OriginalProduct)
        {
            Project p = new Project();
            p.iMonth = MonthID;
            p.CreatedInSafeMode = this.SafeMode;
            p.ID = ++lastProjectID;
            p.OriginalProduct = OriginalProduct;
            p.iMonthStarted = MonthID;
            //p.State = (OriginalProduct == null) ? ProjectState.NotStarted : ProjectState.Spec;
            p.State = ProjectState.NotStarted;
            p.Kind = Kind;
            p.FindingStaff += new FindingStaffEventHandler(OnProjectFindingStaff);
            p.ProjectReleasing += new ProjectReleasingEventHandler(OnProjectReleasing);
            //ProjectCreated(p);
            Debug.WriteLine("A new project created. ID = " + p.ID.ToString() + "; Kind = " + p.Kind.ToString());
            AllProjects.Add(p);
            //p.NextMonth();
            return p;
        }

        void OnProjectReleasing(Project Project)
        {
            LaunchProduct(Project);
            //AllProjects.Remove(Project);
        }
        Staff OnProjectFindingStaff(Type staffType, Project Project)
        {
            Debug.Write("Finding a " + staffType.Name + " for project ID = " + Project.ID + " ... ");
            int minMonth = Project.Kind == ProjectKind.Large ? 6 : 0;
            Staff staff = AllEmployees.Find(c => c.GetType().Equals(staffType) && (Project.Kind != ProjectKind.Large || (Project.Kind == ProjectKind.Large && c.CanBeMentor) ) && c.CanParticipateInMoreProjects() && !Project.CurrentMembers.Exists(m => m == c));
            if (staff != null)
            {
                Debug.WriteLine("Found!");
                return staff;
            }
            else
            {
                if (SafeMode && Project.CreatedInSafeMode)
                {
                    throw new SafeModeException();
                }
                else
                {
                    if (Project.Kind != ProjectKind.Large)
                    {
                        staff = RecruitStaff(staffType);
                    }
                    if (staff == null)
                    {
                        throw new NoStaffException();
                    }
                    else
                    {
                        return staff;
                    }
                }
            }
        }

        //void BusinessCycles_ProjectCreated(Project Project)
        //{
        //    projectCreatedThisMonth++;
        //    Project.FindingStaff += new FindingStaffEventHandler(Project_OnFindingStaff);
        //}

        //public event CanCreateProjectEventHandler CanCreateProject;
        //public event ProjectCreatedEventHandler ProjectCreated;
        //public event ProjectClosed ProjectClosed;

        public override void NextMonth()
        {
            CheckProjectDiversity();

            // Any product to upgrade or close?
            foreach (Product p in AllProducts) p.MonthID = MonthID;

            // Any project to update state?
            foreach (Project p in AllProjects) p.MonthID = MonthID;

            // Remove launched projects
            while (true)
            {
                Project p = AllProjects.Where(c => c.State == ProjectState.Closed).FirstOrDefault();
                if (p == null) break;
                AllProjects.Remove(p);
            }
            // Remove closed  products
            while (true)
            {
                Product p = AllProducts.Where(c => c.Closed == true).FirstOrDefault();
                if (p == null) break;
                AllProducts.Remove(p);
            }

            foreach (Staff staff in AllEmployees) staff.MonthID = MonthID;

            Office.MonthID = MonthID;

            Accounting.MonthID = MonthID;
        }
    }
}
