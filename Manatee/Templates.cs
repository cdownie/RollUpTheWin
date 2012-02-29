using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manatee {
    public class Templates {
        public static string Blank {
            get {
                return @"{
    up:{

    },
    down:{

    }
}";
            }
        }
        public static string CreateTable {
            get {
                return @"{
    up:{
        create_table:{
            name:'my_table',
            columns:[
                {name:'name', type:'string'},
                {name:'description', type:'text'}
             ],
            timestamps:true
         }
    }
}";
            }
        }
        public static string AddColumn {
            get {
                return @"{
    up:{
        add_column:{
            table:'my_table',
            columns:[
                {name:'name', type:string}
            ]
        }
    }
}";
            }
        }
        public static string AddIndex {
            get {
                return @"{
	up:{
		add_index:{
			table_name:'my_table',
            columns:[
                'my_column'
             ]
		}
	}
}";
            }
        }
        public static string FK {
            get {
                return @"{
	up:{
		foreign_key:{
			from_table:'my_table',
            to_table:'my_other_table',
            from_column:'my_table_key',
            to_column:'my_other_table_PK'
		}
	}
}";
            }
        }
    }
}
