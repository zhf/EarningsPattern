using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EarningsPattern2
{
    delegate void ProjectReleasingEventHandler(Project Project);
    delegate Staff FindingStaffEventHandler(Type staffType, Project Project);

    [Serializable]
    enum ProjectState { NotStarted, Analyzing, Spec, Coding, Testing, Packaging, Released, Closed }
    enum ProjectKind { Small, SmallUpgrade, Medium, MediumUpgrade, Large, LargeUpgrade }

    [Serializable]
    class Project : MonthlyVariedObject
    {
        public Project()
        {
            CurrentMembers = new List<Staff>();
        }
        public int ID;
        public bool CreatedInSafeMode;
        public Product OriginalProduct { get; set; }
        public int iMonthStarted { get; set; }
        private ProjectState state;
        public ProjectState State
        {
            get
            {
                return state;
            }
            set
            {
                if (state != value)
                {
                    state = value;
                    iMonthStateUpdated = MonthID;
                }
            }
        }
        public ProjectKind Kind { get; set; }
        public ProductKind TargetProductKind { get; set; }
        private int iMonthStateUpdated;
        public event ProjectReleasingEventHandler ProjectReleasing;
        public void Update()
        {
            int stateDuration = MonthID - iMonthStateUpdated;
            switch (Kind)
            {
                case ProjectKind.Medium:
                    {
                        switch (State)
                        {
                            case ProjectState.NotStarted:
                                State++; // Analyzing
                                AddMembers(typeof(ProductManager), 1);
                                AddMembers(typeof(ProgramManager), 1);
                                AddMembers(typeof(DocumentManager), 1);
                                break;
                            case ProjectState.Analyzing:
                                if (stateDuration >= 1)
                                {
                                    State++; // Spec
                                    RemoveMembers(typeof(ProductManager));
                                }
                                break;
                            case ProjectState.Spec:
                                if (stateDuration >= 1)
                                {
                                    State++; // Coding
                                    AddMembers(typeof(Developer), 2);
                                    AddMembers(typeof(Researcher), 2);
                                }
                                break;
                            case ProjectState.Coding:
                                if (stateDuration >= 2)
                                {
                                    State++; // Testing
                                    //RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Researcher));
                                    AddMembers(typeof(Tester), 1);
                                }
                                break;
                            case ProjectState.Testing:
                                if (stateDuration >= 1)
                                {
                                    State++; // Packaging
                                    AddMembers(typeof(ProductManager), 1);
                                    //AddMembers(typeof(ProgramManager), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;
                            case ProjectState.Packaging:
                                if (stateDuration >= 1)
                                {
                                    State++; // Released
                                    RemoveMembers(typeof(DocumentManager));
                                    RemoveMembers(typeof(ProductManager));
                                    RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Developer));
                                    RemoveMembers(typeof(Tester));
                                    RemoveMembers(typeof(Artist));
                                }
                                break;
                            case ProjectState.Released:
                                State++;
                                ProjectReleasing(this);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        break;
                    }
                case ProjectKind.MediumUpgrade:
                    {
                        switch (State)
                        {
                            case ProjectState.NotStarted:
                                State = ProjectState.Spec; // Skip the step Analyzing
                                AddMembers(typeof(ProgramManager), 1);
                                AddMembers(typeof(DocumentManager), 1);
                                break;
                            case ProjectState.Spec:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Coding;
                                    AddMembers(typeof(Developer), 1);
                                    AddMembers(typeof(Researcher), 1);
                                }
                                break;
                            case ProjectState.Coding:
                                if (stateDuration >= 2)
                                {
                                    State = ProjectState.Testing; // and Packaging
                                    RemoveMembers(typeof(Researcher));
                                    AddMembers(typeof(Tester), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;
                            case ProjectState.Testing:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Released;
                                    RemoveMembers(typeof(DocumentManager));
                                    RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Developer));
                                    RemoveMembers(typeof(Tester));
                                    RemoveMembers(typeof(Artist));
                                }
                                break;
                            case ProjectState.Released:
                                State = ProjectState.Closed;
                                ProjectReleasing(this);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        break;
                    }
                case ProjectKind.Small:
                    {
                        switch (State)
                        {
                            case ProjectState.NotStarted:
                                State++; // Analyzing + Spec
                                AddMembers(typeof(ProductManager), 1);
                                AddMembers(typeof(ProgramManager), 1);
                                AddMembers(typeof(DocumentManager), 1);
                                break;
                            case ProjectState.Analyzing:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Coding;
                                    RemoveMembers(typeof(ProductManager));
                                    AddMembers(typeof(Developer), 1);
                                    AddMembers(typeof(Researcher), 1);
                                }
                                break;
                            case ProjectState.Coding:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Testing; // and Packaging
                                    RemoveMembers(typeof(Researcher));
                                    AddMembers(typeof(Tester), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;
                            case ProjectState.Testing:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Released;
                                    RemoveMembers(typeof(DocumentManager));
                                    RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Developer));
                                    RemoveMembers(typeof(Tester));
                                    RemoveMembers(typeof(Artist));
                                }
                                break;
                            case ProjectState.Released:
                                State++;
                                ProjectReleasing(this);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        break;
                    }
                case ProjectKind.SmallUpgrade:
                    {
                        switch (State)
                        {
                            case ProjectState.NotStarted:
                                State = ProjectState.Coding; // Include Program Manager, skip the step Analyzing
                                AddMembers(typeof(ProgramManager), 1);
                                AddMembers(typeof(DocumentManager), 1);
                                AddMembers(typeof(Developer), 1);
                                break;
                            case ProjectState.Coding:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Testing;// and Packaging
                                    AddMembers(typeof(Tester), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;
                            case ProjectState.Testing:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Released;
                                    RemoveMembers(typeof(DocumentManager));
                                    RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Developer));
                                    RemoveMembers(typeof(Tester));
                                    RemoveMembers(typeof(Artist));
                                }
                                break;
                            case ProjectState.Released:
                                State = ProjectState.Closed;
                                ProjectReleasing(this);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        break;
                    }
                case ProjectKind.Large:
                    {
                        switch (State)
                        {
                            case ProjectState.NotStarted:
                                State++; // Analyzing
                                AddMembers(typeof(ProductManager), 1);
                                AddMembers(typeof(ProgramManager), 1);
                                AddMembers(typeof(DocumentManager), 1);
                                break;
                            case ProjectState.Analyzing:
                                if (stateDuration >= 2)
                                {
                                    State++; // Spec
                                    RemoveMembers(typeof(ProductManager));
                                }
                                break;
                            case ProjectState.Spec:
                                if (stateDuration >= 2)
                                {
                                    State++; // Coding
                                    AddMembers(typeof(Developer), 3);
                                    AddMembers(typeof(Researcher), 2);
                                }
                                break;
                            case ProjectState.Coding:
                                if (stateDuration >= 4)
                                {
                                    State++; // Testing
                                    //RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Researcher));
                                    AddMembers(typeof(Tester), 2);
                                }
                                break;
                            case ProjectState.Testing:
                                if (stateDuration >= 1)
                                {
                                    State++; // Packaging
                                    AddMembers(typeof(ProductManager), 1);
                                    //AddMembers(typeof(ProgramManager), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;
                            case ProjectState.Packaging:
                                if (stateDuration >= 1)
                                {
                                    State++; // Released
                                    RemoveMembers(typeof(DocumentManager));
                                    RemoveMembers(typeof(ProductManager));
                                    RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Developer));
                                    RemoveMembers(typeof(Tester));
                                    RemoveMembers(typeof(Artist));
                                }
                                break;
                            case ProjectState.Released:
                                State++;
                                ProjectReleasing(this);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        break;
                    }
                case ProjectKind.LargeUpgrade:
                    {
                        switch (State)
                        {
                            case ProjectState.NotStarted:
                                State++; // Analyzing
                                AddMembers(typeof(ProductManager), 1);
                                AddMembers(typeof(ProgramManager), 1);
                                AddMembers(typeof(DocumentManager), 1);
                                break;
                            case ProjectState.Analyzing:
                                if (stateDuration >= 1)
                                {
                                    State++; // Spec
                                    RemoveMembers(typeof(ProductManager));
                                }
                                break;
                            case ProjectState.Spec:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Coding;
                                    AddMembers(typeof(Developer), 2);
                                    AddMembers(typeof(Researcher), 1);
                                }
                                break;
                            case ProjectState.Coding:
                                if (stateDuration >= 3)
                                {
                                    State = ProjectState.Testing; // and Packaging
                                    RemoveMembers(typeof(Researcher));
                                    AddMembers(typeof(Tester), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;
                            case ProjectState.Testing:
                                if (stateDuration >= 1)
                                {
                                    State++; // Packaging
                                    AddMembers(typeof(ProductManager), 1);
                                    //AddMembers(typeof(ProgramManager), 1);
                                    AddMembers(typeof(Artist), 1);
                                }
                                break;

                            case ProjectState.Packaging:
                                if (stateDuration >= 1)
                                {
                                    State = ProjectState.Released;
                                    RemoveMembers(typeof(ProductManager));
                                    RemoveMembers(typeof(DocumentManager));
                                    RemoveMembers(typeof(ProgramManager));
                                    RemoveMembers(typeof(Developer));
                                    RemoveMembers(typeof(Tester));
                                    RemoveMembers(typeof(Artist));
                                }
                                break;
                            case ProjectState.Released:
                                State = ProjectState.Closed;
                                ProjectReleasing(this);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        break;
                    }
                default: throw new InvalidOperationException();
            }
                   
        }
        public List<Staff> CurrentMembers;
        public event FindingStaffEventHandler FindingStaff;
        private void AddMembers(Type MemberType, int nStaffs)
        {
            for (int i = 0; i < nStaffs; i++)
            {
                Staff staff = FindingStaff(MemberType, this);
                Debug.WriteLine("Adding " + MemberType.Name + " to project ID = " + this.ID);
                CurrentMembers.Add(staff);
                staff.nParticipatedProjects++;
            }
        }
        private void RemoveMembers(Type MemberType)
        {
            Debug.WriteLine("Removing " + MemberType.Name + "(s) from project ID = " + this.ID);
            while (true)
            {
                Staff staff = CurrentMembers.Where(c => c.GetType().Equals(MemberType)).FirstOrDefault();
                if (staff == null) break;
                CurrentMembers.Remove(staff);
                staff.nParticipatedProjects--;
            }
        }
        public override void NextMonth()
        {
            Update();
        }
    }

}