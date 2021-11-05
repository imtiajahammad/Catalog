namespace Catalog.Settings{

    public class MongoDbSettings{

        public string Host { get; set; }
        public int Port { get; set; }

        public string User { get; set; }
        public string Password { get; set; }
        //public string ReplicaSetName { get; set; }
        public string ConnectionString 
        {
             get
             {
                 var aa=$"mongodb://{User}:{Password}@{Host}:{Port}";
                 return aa;//$"mongodb://{Host}:{Port}";
                 //return $"mongodb://{User}:{Password}@{Host}:{Port}/mongo";
                 //return $"mongodb://{User}:{Password}@{Host}:{Port}";
                 //mongodb://myusername:mypassword@serverip:27017/databasename
             } 
             
        }
    }
}