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

        public  string    bsvInputFile              =  "";
        public  bool      isHeaderRow               =  true;
        public  string    rowDelimiter              =  "";
        public  string    columnDelimiter           =  "";
        public  string    outputFileString          = Directory.GetCurrentDirectory()+"\\";

        public  string    destinationTable          =  "";

        public  static    System.IO.StreamWriter  fs;

        public  bool      parsingXML                = false;

        public  int       columnCount               = 0;


        public  BscParser(){
            new  BscParserUtiLibrary();
            startBscParser();
            
            
        }
      public  BscParser(string configFile){
            new  BscParserUtiLibrary(configFile);
            startBscParser();
            
        }

        public void startBscParser(){

            new  BscParser(BscParserUtiLibrary.sourceFilePath,BscParserUtiLibrary.columnDelimiter,BscParserUtiLibrary.destinationTable,BscParserUtiLibrary.parseOutputSqlFile );

             if(BscParserUtiLibrary.shouldCreateTable){

                 if(File.Exists(BscParserUtiLibrary.parseOutputTableScript)){

                  string  createScript =    File.ReadAllText(BscParserUtiLibrary.parseOutputTableScript).Replace("DESTINATION_TABLE_NAME",BscParserUtiLibrary.destinationTable ).Replace("DESTINATION_DATABASE_NAME",BscParserUtiLibrary.destinationDatabase );
                  BscParserUtiLibrary.executeScript(createScript, BscParserUtiLibrary.getConnectionString());

                 }else {

                     Console.WriteLine("The destination table create script path is not valid: "+BscParserUtiLibrary.parseOutputTableScript );
                      BscParserUtiLibrary.writeToLog("The destination table create script path is not valid: "+BscParserUtiLibrary.parseOutputTableScript);
                 }
                
             }

             switch(BscParserUtiLibrary.tableInsertMode){

                 case BscParserUtiLibrary.SINGLE_INSERT_MODE:
                      if(File.Exists(BscParserUtiLibrary.parseOutputSqlFile)){
                        int counter = 0;  
                        string line;  
                        System.IO.StreamReader file =  new System.IO.StreamReader(BscParserUtiLibrary.parseOutputSqlFile);  
                        while((line = file.ReadLine()) != null)  
                        { 
                             line = " USE "+BscParserUtiLibrary.destinationDatabase+"; "+line;
                            Console.WriteLine("Running command on line: {0} .", counter+": "+line);
                            BscParserUtiLibrary.writeToLog("Running command on line: {0} ."+ counter+": "+line);

                            BscParserUtiLibrary.executeScript(line, BscParserUtiLibrary.getConnectionString());  
                            counter++;  
                       
                        }  

                        file.Close();  
                      
                      }  else{
                        Console.WriteLine("The output script path is not valid: "+BscParserUtiLibrary.parseOutputSqlFile );
                         BscParserUtiLibrary.writeToLog("The output script path is not valid: "+BscParserUtiLibrary.parseOutputSqlFile);


                      }


                 break;
                 case BscParserUtiLibrary.BATCH_INSERT_MODE:

                   StringBuilder  batcher  =  new StringBuilder();
                   int  counter2            = 0;
                   string  line2            = "";
                     System.IO.StreamReader file2 =  new System.IO.StreamReader(BscParserUtiLibrary.parseOutputSqlFile); 

                     while((line2 = file2.ReadLine()) != null)  
                        { 
                             ++counter2;
                             line2 = " USE "+BscParserUtiLibrary.destinationDatabase+"; "+line2;
                            Console.WriteLine("Running command on line: {0} .", counter2+": "+line2);
                            BscParserUtiLibrary.writeToLog("Running command on line: {0} ."+ counter2+": "+line2);
                            
                            batcher.Append(line2 );
                            if(counter2 % BscParserUtiLibrary.batchSize==0){
                               BscParserUtiLibrary.executeScript(batcher.ToString(), BscParserUtiLibrary.getConnectionString());  
                               batcher.Remove(0, batcher.Length);
                            }

                        
                            counter2++;  
                       
                        }  

                        file2.Close();  
                      


                  if(File.Exists(BscParserUtiLibrary.parseOutputSqlFile)){
                      string  insertScript  = File.ReadAllText(BscParserUtiLibrary.parseOutputSqlFile);
                       insertScript =  " USE "+BscParserUtiLibrary.destinationDatabase+"; "+insertScript;
                       BscParserUtiLibrary.executeScript(insertScript, BscParserUtiLibrary.getConnectionString()); 
                  } else{
                        Console.WriteLine("The output script path is not valid: "+BscParserUtiLibrary.parseOutputSqlFile );
                         BscParserUtiLibrary.writeToLog("The output script path is not valid: "+BscParserUtiLibrary.parseOutputSqlFile);


                      }


                 break;
                 default:

                      if(File.Exists(BscParserUtiLibrary.parseOutputSqlFile)){
                      string  insertScript  = File.ReadAllText(BscParserUtiLibrary.parseOutputSqlFile);
                       BscParserUtiLibrary.executeScript(insertScript, BscParserUtiLibrary.getConnectionString()); 
                  } else{
                        Console.WriteLine("The output script path is not valid: "+BscParserUtiLibrary.parseOutputSqlFile );
                         BscParserUtiLibrary.writeToLog("The output script path is not valid: "+BscParserUtiLibrary.parseOutputSqlFile);


                      }
                    break;


             }
            
                BscParserUtiLibrary.closeLogFile();
        }
        public  BscParser(string inFile,  string  colDel,  string  tableName ,string  outFile){

              setInputFile(inFile);
              setOutputFileString(outFile);
              setColumnDelimiter(colDel);
              destinationTable =tableName;
        

              if (!File.Exists(getOutputFileString()))  {
                        
							fs = File.CreateText(getOutputFileString());
					
                    }else{
					
                    		File.Delete(getOutputFileString());
                            fs = File.CreateText(getOutputFileString());
					
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
	              BscParserUtiLibrary.writeToLog("Reading line :"+count+" of "+ (count+csvReader.Peek())+". "+csvReader.Peek()+" records remain.");
				  string  singleLine = csvReader.ReadLine();
				  string[]  dataFields = singleLine.Split(new [] {this.getColumnDelimiter()}, StringSplitOptions.None);
				   
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
                             BscParserUtiLibrary.writeToLog(i.ToString()+": "+dataFields[i]);
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
                                        BscParserUtiLibrary.writeToLog("Parse complete.");
                                        ++colCount;
                                  }


                                }else{
					
                                 
                                             if(dataFields[i].Trim().StartsWith("\"") ){
                                                 paramString.Append("\'");
                                                 while (!dataFields[(i)].Trim().EndsWith("\"")){
                                                    paramString.Append(dataFields[i]);
                                                    ++i;
                                                 }
                                                  paramString.Append(dataFields[i]);
                                                 paramString.Append("\'").Append(",");
                                                ++colCount;
                                              
                                             }else{
									 		paramString.Append("\'").Append(dataFields[i]).Append("\',");
                                             ++colCount;
                                             }
		
                                     
							}	
                            }else {
					           
                               
                            
                               
                                     if(dataFields[i].ToLower().StartsWith("\"<?xml") || parsingXML ){
                                      Console.WriteLine("");
                                      Console.WriteLine("Parsing XML...");
                                       BscParserUtiLibrary.writeToLog("Parsing XML...");
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
							      	paramString.Append("\'"+dataFields[i]+"\'");
                                       ++colCount;
                                }
								
							
							  
						  }
					
						
					
				  }
			     if(colCount == columnCount)   {
                     paramString.Length--;
                      fs.WriteLine("INSERT INTO ["+destinationTable+"] ("+queryString.ToString()+" )VALUES("+paramString.ToString()+")");
                      colCount                     = 0;
                      paramString                  = new StringBuilder();
					   StringBuilder   combiner    = new StringBuilder();
                  }	 
                   
				  } 
	 
			 }
                
            
        } 
        
                    }}catch (Exception ex)
        {
            Console.WriteLine("Error reading table schema: " + ex.Message);
            BscParserUtiLibrary.writeToLog("Error reading table schema: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            BscParserUtiLibrary.writeToLog(ex.StackTrace);
            

        }



            }

            
         fs.Flush();
         fs.Close();   
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
            string  configFile       =  "";

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
						 }else if (args[i].ToLower()=="-c" && (args[(i+1)] != null && args[(i+1)].Length!=0)){
								configFile =   args[(i+1)];							
						}else if (args[0].ToLower()=="-h" ||args[0].ToLower()=="help" || args[0].ToLower()=="/?" || args[0].ToLower()=="?" ){

                        Console.WriteLine(" This application parses bsc files to handle the  XML content");
                        Console.WriteLine(" Usage:  ");	
                         Console.WriteLine("-c: This parameter is used to specify that a JSON  configuration file should be parsed by the application ");
                        Console.WriteLine(" -i: This parameter is used to specify the file to be used. ");
                        Console.WriteLine(" -d: This parameter is used to specify the column delimiter to be used. ");
                        Console.WriteLine(" -o: This parameter is used to specify the output file ");
                        Console.WriteLine(" -t: This parameter is used to specify the table. ");
                        Console.WriteLine(" -h: This parameter is used to print this help message.");	

                       
                                        
                    } 
                }   
 
             if (string.IsNullOrEmpty(configFile)  &&  !(string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(fieldDelimter) || string.IsNullOrEmpty(tableName) ||string.IsNullOrEmpty(outputFile) )  ){

                new BscParser(inputFile, fieldDelimter, tableName, outputFile);

             } else if(!string.IsNullOrEmpty(configFile) ){ 

                    new BscParser(configFile); 

             }else{

                new BscParser(); 

             }
     
               
  
         } catch(Exception e) {
            
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                
         }
				

        }
    }
}
