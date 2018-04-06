﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using PruebaExcel_EPplus;
using OfficeOpenXml;
using System.IO;

namespace SeleniumExcel
{
    class Support
    {
        public string m_strWorkbookName;
        public string m_strWorksheetName;
        public IWebDriver m_iwbWebDriver;
        public LibExcel_epp m_leeExcelObject;

        public string ProprWorkbookName {get => m_strWorkbookName; set => m_strWorkbookName = value;}
        public string ProprWorksheetName { get => m_strWorksheetName; set => m_strWorksheetName = value; }
        public IWebDriver ProprDriver { get => m_iwbWebDriver; set => m_iwbWebDriver = value; }
        public LibExcel_epp PropExcelObject { get => m_leeExcelObject; set => m_leeExcelObject = value; }


        //constructor
        public Support()
        {

        }

        public Support(string pstrWorkbookName, string pstrWorksheetName, IWebDriver piwbDriver, LibExcel_epp pleeExcelObject)
        {
            m_iwbWebDriver = piwbDriver;
            m_leeExcelObject = pleeExcelObject;
            m_strWorkbookName = pstrWorkbookName;
            m_strWorksheetName = pstrWorksheetName;
        }

        //TODO cambiar la manera en la que mete las cosas al Excel para que sea en base al título de la columna y no estático
        //TODO intentar hacerlo todo menos estático
        //TODO poner un método para revisar si existe un archivo de Excel y controlar qué pasa si hay o no hay
        public void SearchGoogle(IWebDriver piwbDriver, LibExcel_epp pleeExcelObject, string pstrWorkbookName, string pstrWorksheetName)
        {
            piwbDriver.Navigate().GoToUrl("http://www.google.com/");

            List<string> headerNames = new List<string>();
            List<string> resultsToSave = new List<string>();
            List<string> searchTerms = new List<string>();
            string searchString = null;//the string we will be using to search

            //GetExcelElements(pleeExcelObject,pstrWorkbookName,pstrWorksheetName,headerNames,searchTerms,resultsToSave);
            //Results(piwbDriver,pleeExcelObject,pstrWorkbookName,pstrWorksheetName,headerNames,searchTerms,resultsToSave,searchString);
            
            piwbDriver.Close();
        }

        public void SearchGoogle()
        {
            m_iwbWebDriver.Navigate().GoToUrl("http://www.google.com/");

            List<string> headerNames = new List<string>();
            List<string> resultsToSave = new List<string>();
            List<string> searchTerms = new List<string>();
            List<string> runElements = new List<string>();

            GetExcelElements(headerNames, searchTerms, resultsToSave, runElements);
            Results(headerNames, searchTerms, resultsToSave,runElements);

            m_iwbWebDriver.Close();
        }

        /// <summary>
        /// Enters a for loop that iterates through all the search strings we want to use, 
        /// as well as the number of saved results we want in the worksheet
        /// </summary>
        /// <param name="piwbDriver"></param>
        /// <param name="pleeExcelObject"></param>
        /// <param name="pstrWorkbookName"></param>
        /// <param name="pstrWorksheetName"></param>
        /// <param name="plHeaderNames"></param>
        /// <param name="plSearchStrings"></param>
        /// <param name="plResultNumbers"></param>
        /// <param name="pstrSearchString"></param>
        public void Results(List<string> plHeaderNames, List<string> plSearchStrings, List<string> plResultNumbers, List<string> plRunElements)
        {
            //This cycle will be used to determine the amount of results to search
            for (int listIndex = 0; listIndex <= plResultNumbers.Count - 1; listIndex++)
            {
                int elementsToSave = int.Parse(plResultNumbers[listIndex]); //converts the strings inside the resultsToSave list into integers we will use to determine how many results we will save for that particular search
                m_iwbWebDriver.FindElement(By.Id("lst-ib")).SendKeys(plSearchStrings[listIndex]); //finds the search bar and sends the string we want to search into it
                    
                /*
                Checks if it's the first time it's searching on Google 
                If true, it looks for the btnK button and clicks it
                If false, it looks for the btnG button and clicks it
                The button changes names depending where you are. 
                */
                if (listIndex == 0)
                {
                    m_iwbWebDriver.FindElement(By.Name("btnK")).Click();
                }
                else
                {
                    m_iwbWebDriver.FindElement(By.Name("btnG")).Click();
                }

                IList<IWebElement> h3Links = m_iwbWebDriver.FindElements(By.ClassName("g")); //saves all the links inside the webpage from the "g" class into an IList
                string totalSearchResults = m_iwbWebDriver.FindElement(By.Id("resultStats")).Text; //gets the total amount of results for that particular search
                IList<IWebElement> relatedResults = m_iwbWebDriver.FindElements(By.ClassName("nVcaUb")); //saves the links for all the related searches results into an IList


                //Sends the data we want to save into the worksheet for the corresponding column 
                //Also gets the lists from before and builds a big string with all the results that were saved in both their hyperlink and text forms
                m_leeExcelObject.Excel_Mod_SingleWFI(m_strWorkbookName, m_strWorksheetName, listIndex + 2, GetColumnIndex(plHeaderNames, "Saved Results Links"), GetResultsHref(h3Links, elementsToSave - 1));
                m_leeExcelObject.Excel_Mod_SingleWFI(m_strWorkbookName, m_strWorksheetName, listIndex + 2, GetColumnIndex(plHeaderNames, "Saved Results Text"), GetResultsTxt(h3Links, elementsToSave - 1));//debe tomar el nombre de la columna donde lo va a poner y el de la fila igual (el término de búsqueda)
                m_leeExcelObject.Excel_Mod_SingleWFI(m_strWorkbookName, m_strWorksheetName, listIndex + 2, GetColumnIndex(plHeaderNames, "Related Results Links"), GetResultsHref(relatedResults, relatedResults.Count - 1));
                m_leeExcelObject.Excel_Mod_SingleWFI(m_strWorkbookName, m_strWorksheetName, listIndex + 2, GetColumnIndex(plHeaderNames, "Related Results Text"), GetResultsTxt(relatedResults, relatedResults.Count - 1));
                m_leeExcelObject.Excel_Mod_SingleWFI(m_strWorkbookName, m_strWorksheetName, listIndex + 2, GetColumnIndex(plHeaderNames, "Total number of results"), GetTotalSearchResults(totalSearchResults));

                m_iwbWebDriver.FindElement(By.Id("lst-ib")).Clear(); //clears the search field when we finish with a search
            }
        }

        /// <summary>
        /// Fills some lists we created so we can use them to know how many search results we will
        /// save, what strings we will be searching for and the amount of searches in general we
        /// wil be doing
        /// </summary>
        /// <param name="pleeExcelObject"></param>
        /// <param name="pstrWorkbookName"></param>
        /// <param name="pstrWorksheetName"></param>
        /// <param name="plHeaderNames"></param>
        /// <param name="plSearchStrings"></param>
        /// <param name="plResultNumbers"></param>
        public void GetExcelElements(List<string> plHeaderNames, List<string> plSearchStrings, List<string> plResultNumbers, List<string> plRunElements)
        {
            FileStream stream = new FileStream(@"E:\" + m_strWorkbookName + ".xlsx", FileMode.Open); //creates a file stream to the file we want to manipulate
            ExcelPackage objExcel = new ExcelPackage();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            objExcel.Load(stream);
            ExcelWorksheet worksheet = objExcel.Workbook.Worksheets[m_strWorksheetName];

            m_leeExcelObject.GetWorksheetHeader(worksheet, plHeaderNames);
            m_leeExcelObject.IterateByColumn(worksheet, GetColumnIndex(plHeaderNames, "Input Parameter"), plSearchStrings);
            m_leeExcelObject.IterateByColumn(worksheet, GetColumnIndex(plHeaderNames, "Number of results to save"), plResultNumbers);
            m_leeExcelObject.IterateByColumn(worksheet, GetColumnIndex(plHeaderNames, "Run"), plRunElements);

            stream.Close();
            stream.Dispose();
            objExcel.Save();
            objExcel.Dispose();
        }

        public int GetColumnIndex(List<string> plList, string pstrColumnName)
        {
            int colIndex = 0;

            try
            {
                if (plList.Contains(pstrColumnName))
                {
                    colIndex = plList.IndexOf(pstrColumnName);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return colIndex + 1;
        }

        public int GetRowIndex(List<string> plList, string pstrRowName)
        {
            int rowIndex = 0;

            try
            {
                if (plList.Contains(pstrRowName))
                {
                    rowIndex = plList.IndexOf(pstrRowName);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return rowIndex + 2;
        }

        /// <summary>
        /// Gets the text atribute for the search results in a google search and returns them as a 
        /// big string
        /// </summary>
        /// <param name="pilResultsList"></param>
        /// <param name="pintLimit"></param>
        /// <returns></returns>
        public string GetResultsTxt(IList<IWebElement> pilResultsList, int pintLimit)
        {
            string ResultString = null;

            for (int index = 0; index <= pintLimit; index++)
            {
                string myURL = pilResultsList[index].FindElement(By.TagName("a")).GetAttribute("text");//gets the text (name) of the link in a certain place in our list
                ResultString = ResultString + myURL + ", ";
            }

            return ResultString;
        }

        /// <summary>
        /// Gets the href atribute for the search results in a google search and returns them
        /// as a big string
        /// </summary>
        /// <param name="pilResultsList"></param>
        /// <param name="pintLimit"></param>
        /// <returns></returns>
        public string GetResultsHref(IList<IWebElement> pilResultsList, int pintLimit)
        {
            string ResultString = null;

            for (int index = 0; index <= pintLimit; index++)
            {
                string myURL = pilResultsList[index].FindElement(By.TagName("a")).GetAttribute("href");//gets the text (name) of the link in a certain place in our list
                ResultString = ResultString + myURL + ", ";
            }

            return ResultString;
        }

        /// <summary>
        /// Gets the total search results string and splits it so that it returns the 
        /// number in the middle as a string
        /// </summary>
        /// <param name="pstrTotalSearchResults"></param>
        /// <returns></returns>
        public string GetTotalSearchResults(string pstrTotalSearchResults)
        {
            string sub = pstrTotalSearchResults.Substring(9);
            string sub2 = sub.Substring(0, sub.Length - 28);

            return sub2;
        }
    }
}
