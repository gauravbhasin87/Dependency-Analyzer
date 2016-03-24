///////////////////////////////////////////////////////////////////////
// Repository.cs - contains the data structure for storing parse and //
//                 analysis information                              //                                                                  //
// ver 1.0                                                           //
// Language:    C#, .Net Framework 4.5                               //
// Platform:    Sony Vaio, Win8.1, SP1                               //
// Application: Demonstration for CSE681, Project #2, Fall 2014      //
// Author:      Gaurav Bhasin, Syracuse University                   //
//              (315) 744-5233, gabhasin@syr.edu                     //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Repository package provides a mechanism to store the information gathered by Parser and Analyser. It provides the data structure for type analysis -Elem and
 * structure for storing relation ship analysis ie ElemForRel.This package also uses ScopeStack required to check on the scopes inside function and also used in
 * actions for type analysis Rules.
 * 
 * Public Interface:
 * ------------------
 *  public string AttachedFile //property
 *  public Repository(string fileName) //constructor
 *  public CSsemi.CSemiExp semi//gives attached semi-expression
 *  public int lineCount//gives line number of semiexpression under process
 *  public ScopeStack<T> stack // property
 *  public List<T> locations // generic list
        
 *  
 */
/* Required Files:
 *   -Parser.cs, RulesAndActions.cs, Repository.cs, Semi.cs
 *   
 *
 *   
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScopeStack;

using CSsemi;

namespace RepositoryNS 
{
    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public int funcComplexity { get; set; }


        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            //temp.Append("{");
            temp.Append(String.Format("{0,-15}", type));
            temp.Append(String.Format("{0,-30}", name));
            temp.Append(String.Format("{0,-15}", begin.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-15}", end.ToString()));    // line of scope end
            temp.Append(String.Format("{0,-15}", funcComplexity.ToString()));
            //temp.Append("}");
            return temp.ToString();
        }
    }

    //data structure for holding relationship analysis information
    public class ElemForRel
    {   
        public string fileName  { get; set; }
        public string Relation  { get; set; }
        public string type1     { get; set; }
        public string type1Name { get; set; }
        public string text      { get; set; }
        public string fileName2 { get; set; }
        public string type2     { get; set; }
        public string type2Name { get; set; }
        public int lineNumber   { get; set; }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
           // temp.Append("{");
            temp.Append(String.Format("{0,-35}", fileName));
            temp.Append(String.Format("{0,-15}", Relation));
            temp.Append(String.Format("{0,-30}", type1));
            temp.Append(String.Format("{0,-25}", type1Name)); 
            temp.Append(String.Format("{0,-15}", text));
            temp.Append(String.Format("{0,-35}", fileName2));
            temp.Append(String.Format("{0,-30}", type2));
            temp.Append(String.Format("{0,-20}", type2Name)); 
            temp.Append(String.Format("{0,-5}", lineNumber.ToString()));
            //temp.Append("}");
            return temp.ToString();
        }

    }

    //genric repository to stored locations list for any data structure
    public class Repository<T>
    {
        ScopeStack<T> stack_;
        List<T> locations_;
        static Repository<T> instance;
        private string attachedFile;
        // files attached to the instance of repository
        public string AttachedFile
        {
            get { return attachedFile; }
            set { attachedFile = value; }
        }

        //constructor
        public Repository(string fileName)
        {
            //instance = this;
            stack_ = new ScopeStack<T>();
            locations_ = new List<T>();
            attachedFile = String.Copy(fileName);

        }

        //default constructor
        public Repository() 
        {
            locations_ = new List<T>();    
        }

        public static Repository<T> getInstance()
        {
            return instance;
        }
        // provides all actions access to current semiExp

        public CSsemi.CSemiExp semi
        {                                                                      
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }
        // enables recursively tracking entry and exit from scopes

        public ScopeStack<T> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
        // the locations table is the result returned by parser's actions
        // in this demo

        public List<T> Locations
        {
            get { return locations_; }
            set { locations_ = value; }
        }

    }
}
