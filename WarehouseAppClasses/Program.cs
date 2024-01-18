using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace WarehouseAppClasses
{
    enum Post
    {
        User,
        Employee,
        Warehouse_Manager,
        Fleet_Manager,
        Ceo
    }
    abstract class Entity
    {
        private Dictionary<Post, string> Tables = new Dictionary<Post, string>();
        private void FillTables()
        {
            Tables.Add(Post.User, "users");
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
            List<string[]> datas = Faker2.SqlQuery($"SELECT name, password, id FROM {Tables[Job]}");
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
            return Tables[this.Job];
        }
    }
    class Ceo : Entity
    {
        public Dictionary<int, string> Warehouses = new Dictionary<int, string>();
        //TODO: Majd ezek a listákat updatelni a classokkal:
        public List<string> Warehouse_Managers = new List<string>();
        public List<string> Fleet_Managers = new List<string>();
        public Ceo(string Email) : base(Email)
        {
            List<string[]> Warehouses_List = Faker2.SqlQuery($"SELECT id, warehouse_name FROM warehouses WHERE manager_id = {this.ID}");
            List<string[]> Warehouse_Managers_List = Faker2.SqlQuery($"SELECT name FROM warehousemanagers WHERE ceo_id = {this.ID}");
            List<string[]> Fleet_Managers_List = Faker2.SqlQuery($"SELECT name FROM fleetmanagers WHERE ceo_id = {this.ID}");
            for (int i = 0; i < Warehouses_List.Count; i++)
            {
                Warehouses.Add(int.Parse(Warehouses_List[i][0]), Warehouses_List[i][1]);
            }
            for (int i = 0; i < Warehouse_Managers_List.Count; i++)
            {
                Warehouse_Managers.Add(Warehouse_Managers_List[i][0]);
            }
            for (int i = 0; i < Fleet_Managers_List.Count; i++)
            {
                Fleet_Managers.Add(Fleet_Managers_List[i][0]);
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Faker2.SetDatabaseName("warehouse_database");
        }
    }
}
