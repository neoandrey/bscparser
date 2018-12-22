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

        public  bool      parsingXML                  = false;

        public  int       columnCount                = 0;


        public  BscParser(){
            
            
        }
     
        public  BscParser(string inFile,  string  rowDel,  string  tableName ,string  outFile){

              setInputFile(inFile);
              setOutputFileString(outFile);
              setRowDelimiter(rowDel);
              destinationTable =tableName;
        

              if (!File.Exists(getOutputFileString()))  {
                        
							fs = File.CreateText(getOutputFileString());
					
                    }else{
					
                    		fs = File.AppendText(getOutputFileString());
					
                    } 

            if (File.Exists(getInputFile())){


	try{
					using(StreamReader  csvReader = new StreamReader (getInputFile())) { 
					 int  count    = 0;
                     int  colCount = 0;
					// System.Data.DataTable csvData =  new DataTable();
					 StringBuilder queryString =  new StringBuilder();
					 StringBuilder paramString  = new StringBuilder();
								
 			 while (csvReader.Peek() >= 0) {
				  ++count;
				  Console.WriteLine("Reading line :"+count+" of "+ (count+csvReader.Peek())+". "+csvReader.Peek()+" records remain.");
	             
				  string  singleLine = csvReader.ReadLine();
				  string[]  dataFields = singleLine.Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries);
				   
				  if(count ==1 && isHeaderRow){

                      columnCount = dataFields.Length;
						
					   for (int i=0;  i<columnCount;  i++){

						 if(i!=(columnCount-1)){

								queryString.Append("["+dataFields[i]+"]").Append(",");
								
								
							}	else {
								queryString.Append("["+dataFields[i]+"]");
								
							}
					   }
						   
						 
				  }else{
						
						 for(int i=0; i< dataFields.Length; i++){

							 Console.WriteLine(i.ToString()+": "+dataFields[i]);
                             Console.WriteLine("colCount: "+colCount.ToString());
                             
							if( colCount != columnCount ){
                                if(dataFields[i].ToLower().StartsWith("\"<?xml") || parsingXML ){
                                       Console.WriteLine("");
                                       Console.WriteLine("Parsing XML...");
                                       Console.WriteLine("");
                                        if(!parsingXML){
                                          paramString.Append("\'").Append(dataFields[i]);
                                          parsingXML = true;
                                      }
                                    
                                    while(!dataFields[i].ToLower().Contains("</repeaterdata>") && i<= (dataFields.Length-1) ){
                                        paramString.Append(dataFields[i]);
                                        ++i;
                                      if(i==dataFields.Length)break;
                                    }
                                    if(i==dataFields.Length)break;
                                  if(dataFields[i].ToLower().Contains("</repeaterdata>"))  {
                                        paramString.Append(dataFields[i]).Append("\',");
                                        parsingXML = false;
                                        Console.WriteLine("Parse complete.");
                                        ++colCount;
                                  }


                                }else{
					
                                 
									// if(i!=(dataFields.Length-1)){

									 		paramString.Append("\'"+dataFields[i]+"\',");
                                             ++colCount;
									/*  }else{
											paramString.Append("\'"+dataFields[i]+"\' ");
										 
									 }
                                     */
							}	
                            }else {
					           
                               
                            
                               
                                     if(dataFields[i].ToLower().StartsWith("\"<?xml") || parsingXML ){
                                      Console.WriteLine("");
                                      Console.WriteLine("Parsing XML...");
                                      Console.WriteLine("");
                                      if(parsingXML==false){

                                          paramString.Append("\'").Append(dataFields[i]);
                                          parsingXML = true;
                                      }
                                    
                                    
                                    while( !dataFields[i].ToLower().Contains("</repeaterdata>")){
                                        paramString.Append(dataFields[i]);
                                        ++i;
                                        if(i==dataFields.Length)break;
                                    }
                                    if(i==dataFields.Length)break;
                                  if(dataFields[i].ToLower().Contains("</repeaterdata>"))  {
                                      paramString.Append(dataFields[i]).Append("\'");
                                      parsingXML = false;
                                        Console.WriteLine("Parse complete.");
                                        ++colCount;
                                  
                                } else{
							      	paramString.Append("\'"+dataFields[i]+"\' ");
                                       ++colCount;
                                }
								
							
							  
						  }
					
						
					
				  }
			     if(colCount == columnCount)   {
                     paramString.Length--;
                      fs.WriteLine("INSERT INTO ["+destinationTable+"] ("+queryString.ToString()+" )VALUES("+paramString.ToString()+")");
                      colCount = 0;
                      paramString                  = new StringBuilder();
					   StringBuilder   combiner     = new StringBuilder();
                  }	 
                   
				  } 
	 
			 }
                
            
        } 
        
                    }}catch (Exception ex)
        {
            Console.WriteLine("Error reading table schema: " + ex.Message);
            Console.WriteLine(ex.StackTrace);

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
       this.outputFileString =  fileName;

    }
     
        static void Main(string[] args) {
            
            string  inputFile        = "";
            string  fieldDelimter    =  "";
            string  tableName        =  "";
            string  outputFile       =  "";

         	try {	
                for(int i =0; i< args.Length; i++){

                    if (args[i].ToLower()=="-i" && (args[(i+1)] != null && args[(i+1)].Length!=0)){
								inputFile =  args[(i+1)];							
						   }else if (args[i].ToLower()=="-d" && (args[(i+1)] != null && args[(i+1)].Length!=0)){
								fieldDelimter =args[(i+1)];							
						   }else if (args[i].ToLower()=="-t" && (args[(i+1)] != null && args[(i+1)].Length!=0)){
								tableName =  args[(i+1)];							
						   						
						   }else if (args[i].ToLower()=="-o" && (args[(i+1)] != null && args[(i+1)].Length!=0)){
								outputFile =   args[(i+1)];							
						}else if (args[0].ToLower()=="-h" ||args[0].ToLower()=="help" || args[0].ToLower()=="/?" || args[0].ToLower()=="?" ){

                        Console.WriteLine(" This application parses bsc files to handle the  XML content");
                        Console.WriteLine(" Usage:  ");	
                        Console.WriteLine(" -i: This parameter is used to specify the file to be used. ");
                        Console.WriteLine(" -d: This parameter is used to specify the column delimiter to be used. ");
                        Console.WriteLine(" -o: This parameter is used to specify the output file ");
                        Console.WriteLine(" -t: This parameter is used to specify the table. ");
                        Console.WriteLine(" -h: This parameter is used to print this help message.");	

                       
                                        
                    } 
                }   

     
               new BscParser(inputFile, fieldDelimter, tableName, outputFile);
  
         } catch(Exception e) {
            
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                
         }
				

        }
    }
}
