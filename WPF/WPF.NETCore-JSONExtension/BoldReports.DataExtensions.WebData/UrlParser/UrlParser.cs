using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Globalization;

namespace BoldReports.Web.DataProviders
{
    /// <summary>
    /// Class basically used to check the template from the string and separate the functions from it and call it in UrlCalendarFunction Class
    /// </summary>
    public class UrlParser
    {
        public string finalDate = "";
        
        int i = 0;
        public List<string> eachFunction1;
        public List<string> outputDate = new List<string>();
        public string userInput = "";
        public string template = "";
        /// <summary>
        /// This function is used to find the templates from the string 
        /// </summary>
        /// <param name="input">The string containing the template</param>
        public void TemplateCount(string input)
        {
            userInput = input;
            input = input.Replace("\r\n", "").Replace("\n","");
            string checker1 = @"{{([^{{]+)}}";
            MatchCollection match = Regex.Matches(input, checker1);
            if (match.Count > 0)
            {
                foreach (Match match1 in match)
                {
                    string checker2 = @"{{:([^{{]+)}}";
                    string checker3 = @"\(.+?\)|\(\)";
                    Match reg = Regex.Match(match1.Value, checker2);
                    if (reg.Success)
                    {
                        if (Regex.Matches(match1.Value, checker3).Count > 0)
                            TemplateSplitter(reg.Groups[1].Value, reg.Groups[0].Value);
                        else
                            throw new Exception("Error in template "+reg.Groups[0].Value+" - No proper function name is there ");
                    }
                    else
                    {
                        throw new Exception("Error in template "+ match1.Value+" -Please check the first colon after the first two open braces");
                    }
                }
            }
        }
        /// <summary>
        /// This function is used to separate the templates into separate methods available
        /// </summary>
        /// <param name="temp">Contains the methodname together</param>
        /// <param name="temp1"> Contains the whole template</param>
        public void TemplateSplitter(string temp,string temp1)
        {
            eachFunction1 = new List<string>();
            bool check = true;
            template = temp1;
            if(!Regex.Match(temp,@"\(([^(])*\)$").Success)
            {
                throw new Exception("Error in template "+temp1 +" -There seems to be function call missing in the last method");
            }
            string eachfunction = @"\:?(.*?)\)\.|\:?(.*?)\)$";
            MatchCollection functions=Regex.Matches(temp, eachfunction);
            foreach(Match function in functions)
            {
                string lastWordChecker = function.Groups[0].Value[function.Groups[0].Value.Length - 1]+"";
                String function1 = "";
                if (lastWordChecker=='.'+"")
                {
                    function1=(function.Groups[0].Value.Remove(function.Groups[0].Value.Length - 1));
                    eachFunction1.Add(function.Groups[0].Value.Remove(function.Groups[0].Value.Length - 1));
                    i++;
                }
                else
                {
                   function1=( function.Groups[0].Value);
                    eachFunction1.Add(function.Groups[0].Value);
                    i++;
                }
            }
            foreach (string function in eachFunction1)
            {  
               
                string param = @"\(\:?(.*?)\):?";
                string param1 = @"\(\)$";
                string check0 = @"((.*?)\.)";
                string check1 = @"(\(:?(.*?)\.)";
                if (Regex.Match(function,param).Success||Regex.Match(function,param1).Success)
                {
                    if(Regex.Matches(function, param).Count>1 || Regex.Matches(function, param1).Count>1)
                    {       
                        throw new Exception("Error in template "+temp1+" -There seems like you forget to add . in between functions in " + function);
                    }
                    if(Regex.Matches(function,check0).Count>0)
                    {
                        MatchCollection dotChecker = Regex.Matches(function, check0);
                        MatchCollection dotBraceChecker = Regex.Matches(function, check1);
                        if (dotChecker.Count!=dotBraceChecker.Count)
                        {
                            throw new Exception("Error in template " + temp1 + " -There seems like you forget to add () in between functions in " + function);
                        }
                    }
                }
                else
                {
                    check = false;
                }
                if (string.IsNullOrWhiteSpace(function))
                {
                    check = false;
                }
                if(check==false)
                {
                    throw new Exception("Error in template " + temp1 + " -There is a problem with "+ function+" as it doesn't contains () ");
                }
            }
            FunctionCaller();
        }
        /// <summary>
        /// This function is used to call the Calendar related functions in the class UrlCalendarFunction
        /// </summary>
        public void FunctionCaller()
        {
            UrlCalendarFunction calu = new UrlCalendarFunction();
            bool check = true;
            foreach (string function in eachFunction1)
            {
                int i = 0;
                string functionName = function.Substring(0, function.IndexOf('('));
                string parameterString = "";
                string param = @"\(\:?(.*?)\)$";
                string param1 = @"\(\)$";   
                if(Regex.Match(function,param).Success)
                {
                    parameterString = Regex.Match(function, param).Groups[1].Value;
                }
                else if (Regex.Match(function, param1).Success)
                {
                    parameterString = Regex.Match(function, param1).Groups[1].Value;
                }
                Type whichType = typeof(UrlCalendarFunction);
                MethodInfo methodInfo = whichType.GetMethod(functionName,BindingFlags.IgnoreCase|BindingFlags.Public|BindingFlags.Instance);
                if (check)
                {
                    if (int.TryParse(parameterString, out i))
                    {
                        try
                        {
                            methodInfo.Invoke(calu, new Object[] { i });
                        }
                        catch (Exception Ex)
                        {
                            calu.updatedTime = DateTime.MinValue;
                            if (string.IsNullOrEmpty(Ex.InnerException + ""))
                            {
                                ParameterExceptions(Ex.GetType() + "", methodInfo, functionName, Ex.Message);
                            }
                            else
                            {
                                ParameterExceptions(Ex.GetType() + "", methodInfo, functionName, Ex.InnerException.Message);
                            }
                        }
                        if (methodInfo.ReturnType == typeof(System.String))
                        {
                            check = false;
                        }
                    }
                    else if (String.IsNullOrEmpty(parameterString))
                    {
                        try
                        {
                            methodInfo.Invoke(calu, null);
                        }
                        catch (Exception Ex)
                        {
                            if (string.IsNullOrEmpty(Ex.InnerException + ""))
                            {
                                ParameterExceptions(Ex.GetType() + "", methodInfo, functionName, Ex.Message);
                            }
                            else
                            {
                                ParameterExceptions(Ex.GetType() + "", methodInfo, functionName, Ex.InnerException.Message);
                            }
                        }
                        if (methodInfo.ReturnType == typeof(System.String))
                        {
                            check = false;

                        }
                    }
                    else
                    {
                        try
                        {
                            methodInfo.Invoke(calu, new Object[] { parameterString });
                        }
                        catch (Exception Ex)
                        {
                            if (string.IsNullOrEmpty(Ex.InnerException+""))
                            {
                                ParameterExceptions(Ex.GetType() + "", methodInfo, functionName, Ex.Message);
                            }
                            else
                            {
                                 ParameterExceptions(Ex.GetType() + "", methodInfo, functionName, Ex.InnerException.Message);
                            }
                        }
                        if (methodInfo.ReturnType == typeof(System.String))
                        {
                            check = false;
                        }
                    }
                }
                else
                {
                    throw new Exception("Error in the template "+template+" -No Method should run after format method");
                }
            }
            if (calu.updatedTime != DateTime.MinValue)    
            {
                if (string.IsNullOrEmpty(calu.formatOutput))
                {
                    calu.Format("MM/dd/yyyy HH:mm");
                }
            }
            outputDate.Add(calu.formatOutput);
            finalDate = calu.formatOutput;
        }
        /// <summary>
        /// This function is called to replace all the templates in the string with the date values. 
        /// </summary>
        public void DateReplacer()
        {
            MatchCollection matches =Regex.Matches(userInput, @"{{[^{{]+}}");
            for(int i=0;i<outputDate.Count;i++)
            {
                userInput=userInput.Replace(matches[i].Groups[0].Value, outputDate[i]);
            }
        }
        /// <summary>
        /// this function is used to check the exceptions and to throw the error message accordingly
        /// </summary>
        /// <param name="e">string variable containint the argument</param>
        /// <param name="methodInfo">Methodinfo to check the type of method</param>
        /// <param name="functionName"></param>
        /// <param name="messages">Some exceptions does contain their own messages</param>
        public void ParameterExceptions(String e, MethodInfo methodinfo,string functionName,string messages)
        {
            switch (e)
            {
                case "System.ArgumentException":
                    ParameterInfo[] array1 = methodinfo.GetParameters();
                    foreach (var parametera in array1)
                    {
                        throw new Exception("Error in template " + template+" -The parameter types for function " + functionName + "() should be " + ParameterType(parametera.ParameterType + ""));          
                    }
                    break;
                case "System.Reflection.TargetParameterCountException":
                    ParameterInfo[] array2 = methodinfo.GetParameters();
                   
                    foreach (var parametera in array2)
                    {
                        throw new Exception("Error in template " + template+" - The parameter for the function "+functionName+"() is of "+ParameterType(parametera.ParameterType+""));
                    }
                    if (array2.Count()==0)
                    {
                        throw new Exception("Error in template " + template + " - The given function " + functionName + " Contains no parameter");  
                    }
                    break;
                case "System.NullReferenceException":
                    throw new Exception("Error in template " + template + " - There is no function named " + functionName);
                case "System.Reflection.TargetInvocationException":
                    throw new Exception("Error in template " + template +"- "+messages);
            }
        }
        /// <summary>
        /// This function is to check the what type of parameter is in the method and when an exception regarding parameter occurs this message is used to check
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public string ParameterType(string parameterInfo)
        {
            string intChecker = @"(Int+)";
            string stringChecker = @"(string+)";
            if (Regex.Match(parameterInfo, intChecker, RegexOptions.IgnoreCase).Success)
            {
                return "Int type";
            }
            else if (Regex.Match(parameterInfo, stringChecker, RegexOptions.IgnoreCase).Success)
            {
                return "string type";
            }
            else
                return null;
        }  

    }
}
