
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
    public class BscParserUtiLibrary
    
    {

                public  static  string                       destinationServer 			         =  "";	
                public  static  string                       destinationDatabase       	         =  "";
                public  static  int                          destinationPort                 	 =   0;
                public  static  string                       destinationTable                    =  "";
                public  static  string                       sourceFilePath                      =  "";
                public  static  System.IO.StreamWriter 	     fs;
                public  static  string 					     logFile						     =  AppDomain.CurrentDomain.BaseDirectory+"..\\log\\bsc_parser_log_"+DateTime.Now.ToString("yyyyMMdd_HH_mm_ss")+".log";
                public  static  string 					     configFileName                      =  AppDomain.CurrentDomain.BaseDirectory+"..\\conf\\bsc_parser_config.json";
                public  static  BscParserConfiguration       bscConfig                           =  new BscParserConfiguration();
                public  static  ConnectionProperty 		     destinationConnectionProps;
                public  static  string                       columnDelimiter                     = "";
                public  static  string                       parseOutputSqlFile                  = "";
                public  static  string                       parseOutputTableScript              = "";
                public  static  bool                         shouldUseTrusted                    = false;
                public  static  bool                         shouldCreateTable                   = false;
                public  static  int                          tableInsertMode                     = 0;
                public  const   int                          SINGLE_INSERT_MODE                  = 0;
                public  const   int                          BATCH_INSERT_MODE                   = 1;
                public  static  string                       destinationConnectionString         = "";
                public  static readonly object               locker                              = new object();
                public  static  int                          batchSize                           = 10;

                public   BscParserUtiLibrary(){

                        initBscParserUtiLibrary();

                }
      			public   BscParserUtiLibrary(string  cfgFile){ 
                      
                      if(!string.IsNullOrEmpty(cfgFile) ){

						   string  nuCfgFile  = "";
                           Console.WriteLine("Logging report activities to file: "+logFile);
                           Console.WriteLine("");
						   Console.WriteLine("Loading configurations in  configuration file: "+cfgFile);
						   nuCfgFile     =  cfgFile.Contains("\\\\")? cfgFile:cfgFile.Replace("\\", "\\\\");

						   try{
							   if(File.Exists(nuCfgFile)){

								configFileName     = nuCfgFile;
								initBscParserUtiLibrary(configFileName);

							   }
						   }catch(Exception e){
							    
								Console.WriteLine("Error reading configuration file: "+e.Message);
								Console.WriteLine(e.StackTrace);
								writeToLog("Error reading configuration file: "+e.Message);
								writeToLog(e.StackTrace);
							
						   }
					   }
				 	
                }


                  public  void  initBscParserUtiLibrary(){

					readConfigFile(configFileName);

                    if (!File.Exists(logFile))  {
                        
							fs = File.CreateText(logFile);
					
                    }else{
					
                    		fs = File.AppendText(logFile);
					
                    } 
                    
					log("===========================Started  BscParser Session at "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"==============================");
				    writeToLog("Configurations have been successfully initialised.");
                    
                    if (String.IsNullOrEmpty(sourceFilePath)){

                        Console.WriteLine("source file has not be  provided");
                        writeToLog("source file has not be  provided");


                    } else if(shouldUseTrusted){

                            setConnectionString("Network Library=DBMSSOCN;Data Source=" + destinationServer+","+destinationPort.ToString()+";database="+destinationDatabase+";Trusted_Connection=True;Connection Timeout=0;Pooling=false;");
   
                    } else if (!String.IsNullOrEmpty(destinationServer) &&  !String.IsNullOrEmpty(destinationDatabase)){
    
                           destinationConnectionProps  = new ConnectionProperty( destinationServer, destinationDatabase );
                           setConnectionString("Network Library=DBMSSOCN;Data Source=" +destinationServer + ","+destinationPort.ToString()+";database="+destinationDatabase+";User id=" + destinationConnectionProps.getSourceUser()+ ";Password=" +destinationConnectionProps.getSourcePassword() + ";Connection Timeout=0;Pooling=false;");

                    } else {

                        Console.WriteLine("Source connection details are not complete");
                        writeToLog("Destination connection details are not complete");

                    }   
               
                }
                public  static string getConnectionString(){
                     return destinationConnectionString;
                }
                public  void setConnectionString(string  conStr){

                      destinationConnectionString  = conStr;


                }
                public  void  initBscParserUtiLibrary(string  config){
					configFileName  = config;
					readConfigFile(configFileName);

                    if (!File.Exists(logFile))  {
                        
							fs = File.CreateText(logFile);
					
                    }else{
					
                    		fs = File.AppendText(logFile);
					
                    } 
                    
					log("===========================Started  BscParser Session at "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"==============================");
				    writeToLog("Configurations have been successfully initialised.");
                    
                    if (String.IsNullOrEmpty(sourceFilePath)){

                        Console.WriteLine("source file has not be  provided");
                        writeToLog("source file has not be  provided");


                     } else if(shouldUseTrusted){

                            setConnectionString("Network Library=DBMSSOCN;Data Source=" + destinationServer+","+destinationPort.ToString()+";database="+destinationDatabase+";Trusted_Connection=True;Connection Timeout=0;Pooling=false;");
   
                    } else if (!String.IsNullOrEmpty(destinationServer) &&  !String.IsNullOrEmpty(destinationDatabase)){
    
                           destinationConnectionProps  = new ConnectionProperty( destinationServer, destinationDatabase );
                           setConnectionString("Network Library=DBMSSOCN;Data Source=" +destinationServer + ","+destinationPort.ToString()+";database="+destinationDatabase+";User id=" + destinationConnectionProps.getSourceUser()+ ";Password=" +destinationConnectionProps.getSourcePassword() + ";Connection Timeout=0;Pooling=false;");

                    } else {

                        Console.WriteLine("Source connection details are not complete");
                        writeToLog("Destination connection details are not complete");

                    }
               
                }
				public  static  void readConfigFile(string configFileName){					

                    Console.WriteLine("Reading configurations from "+configFileName+"  ...");
                    try{

					    string  propertyString            = File.ReadAllText(configFileName);
                        bscConfig                         = Newtonsoft.Json.JsonConvert.DeserializeObject<BscParserConfiguration>(propertyString);  
        
                        destinationServer 			      = bscConfig.destination_server;       	
                        destinationDatabase       	      = bscConfig.destination_database;
                        destinationPort           	      = bscConfig.destination_port;
                        destinationTable                  = bscConfig.destination_table;
                        sourceFilePath                    = bscConfig.source_file_path.Contains("\\\\")? bscConfig.source_file_path:bscConfig.source_file_path.Replace("\\", "\\\\");;
                        shouldUseTrusted                  = bscConfig.use_trusted_authentication_dest;
                        shouldCreateTable                 = bscConfig.should_create_table;
                        parseOutputSqlFile                = bscConfig.parse_output_sql_path.Contains("\\\\")? bscConfig.parse_output_sql_path:bscConfig.parse_output_sql_path.Replace("\\", "\\\\");;
                        parseOutputTableScript            = bscConfig.destination_table_create_script;
                        columnDelimiter                   = bscConfig.column_delimiter;
                        batchSize                         = bscConfig.batch_size;    

                       Console.WriteLine("Configurations have been successfully initialised.");                  

                }catch(Exception e){

                    Console.WriteLine("Error reading configuration file: "+e.Message);
                    Console.WriteLine(e.StackTrace);
                    writeToLog("Error reading configuration file: "+e.Message);
                    writeToLog(e.StackTrace);

                }
            
            }
            public static void  writeToLog(string logMessage){
                lock (locker)
                {
                    fs.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"=>"+logMessage);
                }
            }
			 public static Dictionary<string,string>readJSONMap(ArrayList rawMap){

                    Dictionary<string, string> tempDico = new  Dictionary<string, string>();
                    string tempVal  ="";
                    if(rawMap!=null)
                    foreach(var keyVal in rawMap){
                                
                                   tempVal = keyVal.ToString();
                                   if(!string.IsNullOrEmpty(tempVal)){
                                        tempVal = tempVal.Replace("{","").Replace("}","").Replace("\"","").Trim();
                                        Console.WriteLine("tempVal: "+tempVal);
                                       if(tempVal.Split(':').Count() ==2)tempDico.Add(tempVal.Split(':')[0].Trim(),tempVal.Split(':')[1].Trim());  
                                   }  

                    }
                return tempDico;
            }

                    public static void  executeScript( string script, string  targetConnectionString){

						try{
							using (SqlConnection serverConnection =  new SqlConnection(targetConnectionString)){
									SqlCommand cmd = new SqlCommand(script, serverConnection);
									Console.WriteLine("Executing script: "+script);
									writeToLog("Executing script: "+script);
									cmd.CommandTimeout =0;
									serverConnection.Open();
									cmd.ExecuteNonQuery();
							}
						}catch(Exception e){

									 Console.WriteLine("Error while running script: " + e.Message);
									 Console.WriteLine(e.StackTrace);
									 writeToLog("Error while running script: " + e.Message);
									 writeToLog(e.StackTrace);
									 writeToLog(e.ToString());
									
							}
				}

			public static  void log(string logMessage){
				fs.WriteLine(logMessage);
				fs.Flush();
			}
			public static void closeLogFile(){
                fs.Flush();
				fs.Close();
			}
					
    }
}