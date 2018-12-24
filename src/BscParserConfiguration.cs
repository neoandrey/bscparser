
using System;

using System.Collections;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Data.OleDb;
using System.Configuration;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace bsc_parser
{
    public class BscParserConfiguration
    
    {


        public string       destination_server  { set; get;}
        public string       destination_database  { set; get;}
        public int          destination_port  { set; get;}
        public string       destination_table  { set; get;}
        public string       destination_table_create_script { set; get;}
        public string       source_file_path { set; get;}
        public bool         should_create_table { set; get;}
        public string       parse_output_sql_path { set; get;}
		public bool         use_trusted_authentication_dest { set; get;}
        public string       column_delimiter{set; get;}
        public int          insert_mode {set; get;}

    }
}