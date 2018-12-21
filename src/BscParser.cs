using System;

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

namespace bsc_parser
{

    class   BscParser {

        public  string    bsvInputFile          =  "";
        public  bool      isHeaderRow           =  true;
        public  string    rowDelimiter           =  "";
        public  string    columnDelimiter         =  "";
        public  string    outputFileString         = Directory.GetCurrentDirectory()+"\\";

        public  string    destinationTable        =  "";

        public  static    System.IO.StreamWriter 	    fs;

        public  ArrayList  xmlColumnList          = new ArrayList();



        public  BscParser(){
            
            
        }
     
        public  BscParser(string inFile, string  outFile,  string xmlColumnList  ){

             setInputFile(inFile);
             setOutputFileString(outFile);
             setXMLColumnList(xmlColumnList); 

              if (!File.Exists(getOutputFileString()))  {
                        
							fs = File.CreateText(getOutputFileString());
					
                    }else{
					
                    		fs = File.AppendText(getOutputFileString());
					
                    } 

            if (File.Exists(getInputFile())){


	try{
					using(StreamReader  csvReader = new StreamReader (getInputFile())) { 
					 int count = 0;
					// System.Data.DataTable csvData =  new DataTable();
					 StringBuilder queryString =  new StringBuilder();
					 StringBuilder paramString  = new StringBuilder();
								
 			 while (csvReader.Peek() >= 0) {
				  ++count;
				  Console.WriteLine("Reading line :"+count+" of "+ (count+csvReader.Peek())+". "+csvReader.Peek()+" records remain.");
	             
				 string  singleLine = csvReader.ReadLine();
				  string[]  dataFields = singleLine.Split(new [] {this.getColumnDelimiter()}, StringSplitOptions.RemoveEmptyEntries);
				   
				  if(count ==1 && isHeaderRow){
						
					   for (int i=0;  i<dataFields.Length;  i++){

						 if(i!=(dataFields.Length-1)){

								queryString.Append("["+dataFields[i]+"]").Append(this.getColumnDelimiter());
								
								
							}	else {
								queryString.Append("["+dataFields[i]+"]");
								
							}
					   }
						   
						 
				  }else{
						
						paramString                  = new StringBuilder();
					    string currentCombinedField  = "";
						StringBuilder   combiner     = new StringBuilder();
						
						 for(int i=0; i< dataFields.Length; i++){

							 Console.WriteLine(i.ToString()+": "+dataFields[i]);
                             
							if(i!=(dataFields.Length-1)){
							   if(dataFields[i].StartsWith("\"") && !dataFields[i].EndsWith("\"") )
                               {      combiner.Append("\'").Append(dataFields[i]);
										  ++i;
										  while(   !dataFields[i].StartsWith("\"") && !dataFields[i].EndsWith("\"")){
											  combiner.Append(this.getColumnDelimiter()).Append(dataFields[i]);
										  ++i;

										  }
										  if(dataFields[i].EndsWith("\"")){
											   combiner.Append(getColumnDelimiter()).Append(dataFields[i]).Append("\'").Append(getColumnDelimiter());
										  }
                                           	currentCombinedField=combiner.ToString();
										    Console.WriteLine("Current combined field: "+currentCombinedField);
									
                                            combiner =  new StringBuilder();
								 }  else 
                                 {
									 if(i!=(dataFields.Length-1)){

									 		paramString.Append("\'"+dataFields[i]+"\' ").Append(getColumnDelimiter());
									 }else{
											paramString.Append("\'"+dataFields[i]+"\' ");
										 
									 }

								 }
							   
							
							}	else {
					           
								
								paramString.Append("\'"+dataFields[i]+"\' ");
								
							}
							  
						  }
					
						fs.WriteLine("INSERT INTO ["+destinationTable+"] ("+queryString.ToString()+" )VALUES("+paramString.ToString()+")");
					
				  }
			 
				  } 
						  
			 }
                
            
        } catch (Exception ex)
        {
            Console.WriteLine("Error reading table schema: " + ex.Message);
            Console.WriteLine(ex.StackTrace);

        }



            }

            
            
        }


    public void setXMLColumnList(string  xmlList){

        string[]  columns  = xmlList.Split(',');

        xmlColumnList = new ArrayList();
        int colNum   = 0;

          foreach (var item in columns)
          {
           
          if(int.TryParse(item, out colNum)) {
              colNum = colNum-1;
               xmlColumnList.Add(colNum); 
          }
        
            
          }


    }
   public  string getRowDelimiter(){
        
       return this.rowDelimiter;
   }

  public void  setRowDelimiter(string delimiter){

      this.rowDelimiter = delimiter;
  }


  
   public  string getColumnDelimiter(){
        
       return this.columnDelimiter;
   }

  public void  setColumnDelimiter(string delimiter){

      this.columnDelimiter = delimiter;
  }


        public void setInputFile(string  inFile ){

               this.bsvInputFile = inFile;

        }
        public string getInputFile(){

             return this.bsvInputFile; 
        }

   public  string  getOutputFileString(){
       return  this.outputFileString;
   }
    
    public  void setOutputFileString (string  fileName){
       this.outputFileString =  this.outputFileString +fileName+".sql";

    }
     
        static void Main(string[] args) {

         	try {	
                for(int i =0; i< args.Length; i++){

                    if (args[0].ToLower()=="-h" ||args[0].ToLower()=="help" || args[0].ToLower()=="/?" || args[0].ToLower()=="?" ){

                        Console.WriteLine(" This application parses bsc files to handle the  XML content");
                        Console.WriteLine(" Usage:  ");	
                        Console.WriteLine(" -f: This parameter is used to specify the file to be used. ");
                        Console.WriteLine(" -d: This parameter is used to specify the column delimiter to be used. ");
                        Console.WriteLine(" -r: This parameter is used to specify the row delimiter to be used. ");
                        Console.WriteLine(" -t: This parameter is used to specify the table. ");
                        Console.WriteLine(" -x: This parameter is used to xml columns numbers. ");
                        Console.WriteLine(" -h: This parameter is used to print this help message.");	

                                        
                    } else{

                         Console.WriteLine("Insufficient number of  arguments");

                    } 
                }   


                
         } catch(Exception e) {
            
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                
         }
				

        }
    }
}
