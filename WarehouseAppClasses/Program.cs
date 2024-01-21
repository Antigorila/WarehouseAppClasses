using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace WarehouseAppClasses
{
    //Az enumok igazából csak arra kellenek itt hogy egyszerűbb legyen majd logikai elágazásoknál hivatkozni rájuk
    enum Post
    {
        Employee,
        Warehouse_Manager,
        Fleet_Manager,
        Ceo
    }
    enum EmployeePositions
    {
        Warehouseman,
        Delivery,
        Uploader,
        Purchaser
    }
    abstract class Entity
    {
        private Dictionary<Post, string> Tables = new Dictionary<Post, string>();
        private void FillTables()
        {
            Tables.Add(Post.Employee, "employees");
            Tables.Add(Post.Warehouse_Manager, "warehousemanagers");
            Tables.Add(Post.Fleet_Manager, "fleetmanagers");
            Tables.Add(Post.Ceo, "ceos");
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Post Job { get; set; }

        public Entity(string Email)
        {
            FillTables();
            this.Email = Email;
            Job =  GetPost(Email);
            List<string[]> datas = Faker2.SqlQuery($"SELECT name, password, id FROM {Tables[Job]} WHERE email = '{Email}'");
            Name = datas[0][0];
            Password = datas[0][1];
            ID = int.Parse(datas[0][2]);
        }

        private Post GetPost(string Email)
        {
            foreach (var table in Tables)
            {
                List<string[]> lis = Faker2.SqlQuery($"SELECT email FROM {table.Value}");
                for (int i = 0; i < lis.Count; i++)
                {
                    if (lis[i][0] == Email)
                    {
                        return table.Key;
                    }
                }
            }
            throw new Exception("This person have no job");
        }

        public string GetJobString()
        {
            string EB = Tables[this.Job][0].ToString().ToUpper();
            return EB + Tables[this.Job].Substring(1, Tables[this.Job].Length - 2);
        }
    }
    class Ceo : Entity
    {
        public List<WarehouseManager> Warehouse_Managers()
        {
            List<WarehouseManager> Lis = new List<WarehouseManager>();
            List<string[]> Warehouse_Managers_List = Faker2.SqlQuery($"SELECT email FROM warehousemanagers WHERE ceo_id = {this.ID}");
            for (int i = 0; i < Warehouse_Managers_List.Count; i++)
            {
                Lis.Add(new WarehouseManager(Warehouse_Managers_List[i][0]));
            }
            return Lis;
        }
        public List<FleetManager> FleetManagers()
        {
            List<FleetManager> Lis = new List<FleetManager>();
            List<string[]> FleetManagers_List = Faker2.SqlQuery($"SELECT email FROM fleetmanagers WHERE ceo_id = {this.ID}");
            for (int i = 0; i < FleetManagers_List.Count; i++)
            {
                Lis.Add(new FleetManager(FleetManagers_List[i][0]));
            }
            return Lis;
        }

        public List<Warehouse> Warehouses()
        {
            List<Warehouse> Lis = new List<Warehouse>();
            List<string[]> Warehouses_List = Faker2.SqlQuery($"SELECT warehouse_name FROM warehouses WHERE ceo_id = {this.ID}");
            for (int i = 0; i < Warehouses_List.Count; i++)
            {
                Lis.Add(new Warehouse(Warehouses_List[i][0]));
            }
            return Lis;
        }
        public Ceo(string Email) : base(Email) { }
    }
    class WarehouseManager : Entity
    {
       public WarehouseManager(string Email) : base(Email) { }
        public Ceo CEO()
        {
            int ceo_id = int.Parse(Faker2.SqlQuery($"SELECT ceo_id FROM warehousemanagers WHERE id = {this.ID}")[0][0]);
            return new Ceo(Faker2.SqlQuery($"SELECT email FROM ceos WHERE ceos.id = {ceo_id}")[0][0]);
        }

        public Warehouse Warehouse()
        {
            return new Warehouse(Faker2.SqlQuery($"SELECT warehouse_name FROM warehouses WHERE manager_id = {this.ID}")[0][0]);
        }
    }
    class FleetManager : Entity
    {
        public FleetManager(string Email) : base(Email) { }
        public Ceo CEO()
        {
            int ceo_id = int.Parse(Faker2.SqlQuery($"SELECT ceo_id FROM warehousemanagers WHERE id = {this.ID}")[0][0]);
            return new Ceo(Faker2.SqlQuery($"SELECT email FROM ceos WHERE ceos.id = {ceo_id}")[0][0]);
        }

        public Warehouse Warehouse()
        {
            return new Warehouse(Faker2.SqlQuery($"SELECT warehouse_name FROM warehouses WHERE fleetmanager_id = {this.ID}")[0][0]);
        }
    }
    class Employee : Entity
    {
        private Dictionary<EmployeePositions, string> Positions = new Dictionary<EmployeePositions, string>();
        public EmployeePositions Position { get; set; }
        private void FillPositions()
        {
            Positions.Add(EmployeePositions.Warehouseman, "Raktáros");
            Positions.Add(EmployeePositions.Delivery, "Szállító");
            Positions.Add(EmployeePositions.Uploader, "Feltöltő");
            Positions.Add(EmployeePositions.Purchaser, "Beszerző");
        }
        public string GetPositionString() //Ez vissza add egy megjelenithető formátumban lévő munkát, pl ha a pali warhouseman-ként dolgozik akkor vissz adja hogy 'Raktáros'
        {
            return Positions[this.Position];
        }
        private void GetEmployeePosition()
        {
            string position = Faker2.SqlQuery($"SELECT employees.position FROM employees WHERE id = {this.ID}")[0][0];
            foreach (var pos in Positions)
            {
                if (position == pos.Value)
                {
                    this.Position = pos.Key;
                }
            }
        }

        public Warehouse Warehouse()
        {
            int warehouseID = int.Parse(Faker2.SqlQuery($"SELECT warehouse_id FROM employees WHERE id = {this.ID}")[0][0]);
            return new Warehouse(Faker2.SqlQuery($"SELECT warehouse_name FROM warehouses WHERE id = {warehouseID}")[0][0]);
        }
        public int Payment { get; set; }
        public DateTime userCreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LoginNow { get; set; }
        public string profile_picture { get; set; }
        public Employee(string Email) : base(Email)
        {
            FillPositions();
            GetEmployeePosition();
            List<string[]> Lis = Faker2.SqlQuery($"SELECT payment, user_created_at, last_login, profile_picture FROM employees WHERE id = {this.ID}");
            Payment = int.Parse(Lis[0][0]);
            userCreatedAt = DateTime.Parse(Lis[0][1]);
            LastLogin = DateTime.Parse(Lis[0][2]);
            LoginNow = DateTime.Now;
            profile_picture = Lis[0][3];
        }
    }

    class Warehouse
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double TotalIncome { get; set; }
        public string CityName { get; set; }
        public double City_Longitude { get; set; } //Hosszúsági koórdináta
        public double City_Latitude { get; set; } //Szélleségi koórdináta
        public Warehouse(string WarehouseName) //Raktár név kell az azonositáshoz mivel az adatbázisban ez unique érték
        {
            Name = WarehouseName;
            List<string[]> IDs = Faker2.SqlQuery($"SELECT id, city_id FROM warehouses WHERE warehouses.warehouse_name = '{this.Name}'");
            this.ID = int.Parse(IDs[0][0]);
            int cityID = int.Parse(IDs[0][1]);
            SetIncome();
            List<string[]> datas = Faker2.SqlQuery($"SELECT city_name, longitude, latitude FROM cities WHERE id = {cityID}");
            this.CityName = datas[0][0];
            this.City_Longitude = double.Parse(datas[0][1]);
            this.City_Latitude = double.Parse(datas[0][2]);
        }

        private void SetIncome()
        {
            List<string[]> Income = Faker2.SqlQuery($"SELECT income FROM warehouses WHERE warehouses.warehouse_name = '{this.Name}'");
            try
            {
                this.TotalIncome = double.Parse(Income[0][0]);
            }
            catch (Exception)
            {
                this.TotalIncome = 0;
            }
        }
        public double GetEmployeesSalarySum()
        {
            int allSalary = 0;
            for (int i = 0; i < this.Employees().Count; i++)
            {
                allSalary += this.Employees()[i].Payment;
            }
            return allSalary;
        }
        public WarehouseManager WarehouseManager()
        {
            int warehouseManagerID = int.Parse(Faker2.SqlQuery($"SELECT manager_id FROM warehouses WHERE id = {this.ID}")[0][0]);
            string warehouseManagerEmial = Faker2.SqlQuery($"SELECT warehousemanagers.email FROM warehousemanagers WHERE id = {warehouseManagerID}")[0][0];
            return new WarehouseManager(warehouseManagerEmial);
        }

        public FleetManager FleetManager()
        {
            int fleetManagerID = int.Parse(Faker2.SqlQuery($"SELECT fleetmager_id FROM warehouses WHERE id = {this.ID}")[0][0]);
            string FleetmanagerEmial = Faker2.SqlQuery($"SELECT fleetmanagers.email FROM fleetmanagers WHERE id = {fleetManagerID}")[0][0];
            return new FleetManager(FleetmanagerEmial);
        }

        public Ceo CEO()
        {
            int ceoID = int.Parse(Faker2.SqlQuery($"SELECT ceo_id FROM warehouses WHERE id = {this.ID}")[0][0]);
            string CEOemial = Faker2.SqlQuery($"SELECT ceos.email FROM ceos WHERE id = {ceoID}")[0][0];
            return new Ceo(CEOemial);
        }

        public List<Employee> Employees()
        {
            List<Employee> returnList = new List<Employee>();
            List<string[]> Lis = Faker2.SqlQuery($"SELECT email FROM employees WHERE employees.warehouse_id = {this.ID}");
            for (int i = 0; i < Lis.Count; i++)
            {
                returnList.Add(new Employee(Lis[i][0]));
            }
            return returnList;
        }

        public List<int> OrdersID()
        {
            List<int> returnList = new List<int>();
            List<string[]> Lis = Faker2.SqlQuery($"SELECT id FROM orders WHERE orders.warehouse_id = {this.ID}");
            for (int i = 0; i < Lis.Count; i++)
            {
                returnList.Add(int.Parse(Lis[i][0]));
            }
            return returnList;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            //Itt kell beálitani az adatbázisos könyvárnak az adatbázisának a nevét...ha ezt nem állítjátok be...akkor nem fog működni.
            Faker2.SetDatabaseName("warehouse_database");

            Console.ReadKey();
        }
    }
}
