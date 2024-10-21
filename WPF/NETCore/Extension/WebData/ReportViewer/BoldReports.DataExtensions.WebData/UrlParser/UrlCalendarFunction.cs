﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;

namespace BoldReports.Web.DataProviders
{
    /// <summary>
    /// Calendar class contains all the functions required for calendar manipulations including adddays,addmonths..etc
    /// </summary>
    class UrlCalendarFunction
    {
        public DateTime updatedTime;
        public string formatOutput=string.Empty;
        /// <summary>
        /// today is used to present date and time
        /// </summary>
        /// <returns>DateTime Variable Update_Me</returns>
        public DateTime Today()
        {
            updatedTime = DateTime.Now;  
            return updatedTime;
        }
        /// <summary>
        /// Used to add number of days to present datetime variable Update_Me
        /// </summary>
        /// <param name="addDays"> should not be 0</param>
        /// <returns>DateTime variable Update_Me</returns>
        public DateTime AddDays(int addDays)
        {
            if(!TodayFirst())
            {
                if (addDays == 0)
                {
                    throw new Exception("You are not allowed to enter 0 as parameterr for AddDays method");           
                }
                else
                {
                    updatedTime = updatedTime.AddDays(addDays);
                }
            }
         return updatedTime;
        }
        /// <summary>
        /// Used to add the number of months with the Update_Me datetime variable
        /// </summary>
        /// <param name="addMonths">Should not be 0</param>
        /// <returns>DateTime variable</returns>
        public DateTime AddMonths(int addMonths)
        {
            if(!TodayFirst())
            {
                if (addMonths == 0)
                {
                   throw new Exception("You are not allowed to enter 0 as parameterr for AddMonths method");
                }
                else
                {
                    updatedTime = updatedTime.AddMonths(addMonths);
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// adds Number of Years to the present value of Update_Me
        /// </summary>
        /// <param name="addYears">It should not be 0</param>
        /// <returns>DateTime variable</returns>
        public DateTime AddYears(int addYears)
        {
            if(!TodayFirst())
            {
                if (addYears == 0)
                {
                    throw new Exception("You are not allowed to enter 0 as parameterr for AddYears method");
                }
                else
                {
                    updatedTime = updatedTime.AddYears(addYears);
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// Adds the number of hours to the current value of datetime variable Update_Me
        /// </summary>
        /// <param name="addHours">It should not be 0</param>
        /// <returns>DateTime variable</returns>
        public DateTime AddHours(int addHours)
        {
            if(!TodayFirst())
            {
                if (addHours == 0)
                {
                    throw new Exception("You are not allowed to enter 0 as parameterr for AddHours method");   
                }
                else
                {
                    updatedTime = updatedTime.AddHours(addHours);
                }
            } 
            return updatedTime;
        }
        /// <summary>
        /// Adds the number of minutes to the current value of datetime variable Update_Me
        /// </summary>
        /// <param name="addMinutes">It should not be 0</param>
        /// <returns>DateTime variable</returns>
        public DateTime AddMinutes(int addMinutes)
        {
           if(!TodayFirst())
            {
                if (addMinutes == 0)
                {
                    throw new Exception("You are not allowed to enter 0 as parameterr for AddMinutes method");  
                }
                else
                {
                    updatedTime = updatedTime.AddMinutes(addMinutes);
                }
            }
            return updatedTime;
        }
        /// <summary>
        ///  Adds the number of Weeks to the current value of datetime variable Update_Me
        /// </summary>
        /// <param name="addWeeks"></param>
        /// <returns></returns>
        public DateTime AddWeeks(int addWeeks)
        {
            if(!TodayFirst())
            {
                if (addWeeks == 0)
                {
                    throw new Exception("You are not allowed to enter 0 as parameterr for AddWeeks method");           
                }
                else
                {
                    updatedTime = updatedTime.AddDays(addWeeks * 7);
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// Adds the number of Quarters to the current value of datetime variable Update_Me
        /// </summary>
        /// <param name="addQuarters">It should not be 0</param>
        /// <returns>DateTime variable</returns>
        public DateTime AddQuarters(int addQuarters)
        {
            if(!TodayFirst())
            {
                if (addQuarters == 0)
                {
                    throw new Exception("You are not allowed to enter 0 as parameterr for AddQuarters method");         
                }
                else
                {
                    updatedTime = updatedTime.AddMonths(addQuarters * 3);
                }
            }
            return DateTime.Now;
        }
        /// <summary>
        /// function set the timezone to the timezone provided  by the user.
        /// </summary>
        /// <param name="timeZone">Time zone name in string</param>
        /// <returns>updated date time variable with updated timezone</returns>
        public DateTime SetTimeZone(String timeZone)
        {
            if(!TodayFirst())
            {
                System.Globalization.CultureInfo.GetCultureInfo("IST");
               var requiredTimeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(i => i.Id.Equals(timeZone, StringComparison.CurrentCultureIgnoreCase));
                if (String.IsNullOrEmpty(requiredTimeZone+""))
                {
                    updatedTime = DateTime.MinValue;
                    throw new Exception("Please consider rewriting the timezone correctly as parameter for setTimeZone");
                }
                else
                {
                    updatedTime = updatedTime.ToUniversalTime();
                    updatedTime = TimeZoneInfo.ConvertTime(updatedTime, requiredTimeZone);
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// Used to update the present DateTime variable Update_Me to the month which the user has entered int value of 
        /// </summary>
        /// <returns>Update_Me with date as starting of the month</returns>
        public DateTime SetMonthStart(int setMonthStart)
        {
            if (!TodayFirst())
            {
                if(setMonthStart>12||setMonthStart<1)
                {
                    throw new OutOfMemoryException("Please enter parameter in between 1 and 12 for the method setmonthstart");
                }
                else
                {
                    int months = updatedTime.Month;
                    int monthDifference = setMonthStart - months;
                    updatedTime = updatedTime.AddMonths(monthDifference);
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// Used to set the day of the week as the current date, i.e SetDaysStart(0) => changes the current date to the sunday of the present week.
        /// </summary>
        /// <param name="setDayStart"></param>
        /// <returns>Update_Me DateTime variable gets updated with the date changed as per the set_Day_Start</returns>
        public DateTime SetDayStart(int setDayStart)
        {
            if(!TodayFirst())
            {
                if (setDayStart > 6 || setDayStart < 0)
                { 
                    throw new OutOfMemoryException("Please enter parameter in between 0 and 6 for the method setdaystart");
                }
                else
                {
                    int days = (int)updatedTime.DayOfWeek;
                    int difference = setDayStart - days;
                    updatedTime = updatedTime.AddDays(difference);
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// Used to change the datetime to the starting of the week,month,year or quarter
        /// </summary>
        /// <param name="value">String value telling which starting to be set of</param>
        /// <returns>Update_Me DateTime variable gets updated with the start of provided parameter</returns>
        public DateTime Start(string value)
        {
            if(!TodayFirst())
            {
                if (Contains(value, "week"))
                {
                    int adder = 0 - (int)updatedTime.DayOfWeek;
                    updatedTime = updatedTime.AddDays(adder);
                    updatedTime = new DateTime(updatedTime.Year, updatedTime.Month, updatedTime.Day, 0, 0, 0);
                }
                else if (Contains(value, "month"))
                {
                    updatedTime = new DateTime(updatedTime.Year, updatedTime.Month, 1);
                }
                else if (Contains(value, "year"))
                {
                    updatedTime = new DateTime(updatedTime.Year, 1, 1);
                }
                else if (Contains(value, "quarter"))
                {
                    int presentQuarter = (updatedTime.Month - 1) / 3 + 1;
                    int firstMonthPresentQuarter = (presentQuarter - 1) * 3 + 1;
                    updatedTime = new DateTime(updatedTime.Year, firstMonthPresentQuarter, 1);
                }
                else
                {
                    updatedTime = DateTime.MinValue;
                    throw new Exception("The Start method should have one of these parameters-'week','month','year,'quarter'");
                }
            }
            return updatedTime;
        }
        /// <summary>
        /// This functions converts the date to the ending of week,month,year or quarter
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Update_Me date time variable is updated to the start of whichever parameter user inputs</returns>
        public DateTime End(string value)
        {
            if(!TodayFirst())
            {
                if (Contains(value, "week"))
                {
                    int adder = 6 - (int)updatedTime.DayOfWeek;
                    updatedTime = updatedTime.AddDays(adder);
                    updatedTime = new DateTime(updatedTime.Year, updatedTime.Month, updatedTime.Day, 0, 0, 0);
                }
                else if (Contains(value, "month"))
                {
                    updatedTime = new DateTime(updatedTime.Year, updatedTime.Month, DateTime.DaysInMonth(updatedTime.Year, updatedTime.Month));
                }
                else if (Contains(value, "year"))
                {
                    updatedTime = new DateTime(updatedTime.Year, 12, 31);
                }
                else if (Contains(value, "quarter"))
                {
                    int presentQuarter = (updatedTime.Month - 1) / 3 + 1;
                    int firstMonthPresentQuarter = (presentQuarter - 1) * 3 + 1;
                    updatedTime = new DateTime(updatedTime.Year, firstMonthPresentQuarter, 1);
                    updatedTime = updatedTime.AddMonths(3).AddDays(-1);
                }
                else
                {
                    updatedTime = DateTime.MinValue;
                    throw new Exception("The End method should have one of these parameters-'week','month','year,'quarter'");
                }
            }
            return DateTime.Now;
        }
        /// <summary>
        /// Function to check that incoming variable string value
        /// </summary>
        /// <param name="check"></param>
        /// <param name="toCheck"></param>
        /// <returns>bool value according to match</returns>
        public bool Contains(string check,string toCheck)
        {   
            string param =string.Format( @"^\b{0}\b$", toCheck);
            if (Regex.Match(check, param, RegexOptions.IgnoreCase).Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Format stamp is to format the datetime value as per the formats provided by user in the string
        /// </summary>
        /// <param name="condition">Should contain date formats</param>
        /// <returns>String value and saves the string output in format_output</returns>
        public string Format(string condition)
        {
           if(!TodayFirst())
            {
                if (Contains(condition, "Unixtime")|| Contains(condition, "epoch"))
                {
                    updatedTime = updatedTime.ToUniversalTime();
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);
                    TimeSpan epochTime;
                    epochTime = updatedTime - epoch;
                    var unixTimeFormat = (int)epochTime.TotalSeconds;
                    formatOutput = unixTimeFormat + "";
                    return unixTimeFormat + "";
                }
                else
                {
                    try
                    {
                        formatOutput = CustomFormatChecker(condition);
                        return formatOutput;
                    }
                    catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            return null;   
        }
        /// <summary>
        /// This function is used to check for custom formats that is not supported in C# along with casing
        /// </summary>
        /// <param name="date">Contains the format entered by user</param>
        /// <returns>The date time value in correct format</returns>
        public string CustomFormatChecker(string date)
        {
            string checkDate = date;
            checkDate = Regex.Replace(date, @"(?:'|"").*(?:'|"")", "");
            checkDate = checkDate.Replace('D', 'd');
            checkDate = checkDate.Replace('Y', 'y');
            checkDate = checkDate.Replace('S', 's');
            checkDate = checkDate.Replace('T', 't');
            checkDate = checkDate.Replace('Z', 'z');
            checkDate = checkDate.Replace('k', 'K');
            checkDate = checkDate.Replace('G', 'g');
            string allowable = "zdstyFfKhHMmHg:";
            foreach (char c in checkDate)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (!allowable.Contains(c.ToString()))
                    {
                        throw new Exception("invalid date format");
                    }
                }
            }
            string checkMonth = @"((m+)(?=[ ]([hmstf]))|(m+)(?=[-<>.!@#%/\s](d+))|(m+)(?=d+)|(m+)(?=[-<>.!@#%/\s](y+))|(m+)(?=y+)|(?<=(y+)[-<>.!@#%/\s])(m+)(?=[^:m])|(?<=y+)(m+)(?=[^:m])|(?<=(d+)[-<>.!@#%/\s])(m+)(?=[^:m])|(?<=d+)(m+)|(?<=(M+)[-<>.!@#%/\s])(m+)|(?<=M+)(m+)|(m+)(?=[-<>.!@#%/\s](M+))|(m+)(?=M+))";
            string checkMinutes = @"(?<=[^-/M])(M+)(?=[-<>.!@#%:]h+)|^(M+)(?=[-<>.!@#%:]h+)|(M+)(?=h+)|(?<=[^/M])(M+)(?=[-<>.!@#%/:]H+)|^(M+)(?=[<>.!@#%:]H+)|(M+)(?=H+)|(?<=h+[<>.!@#%:])(M+)|(?<=h+)(M+)|(?<=H+[<>.!@#%:])(M+)|(?<=H+)(M+)|(M+)(?=[-<>.!@#%/:]s+)|(M+)(?=s+)|(M+)(?=[-<>.!@#%/:]t+)|(M+)(?=t+)|(?<=(s+)[-<>.!@#%/:])(M+)|(?<=s+)(M+)|(?<=(t+)[-<>.!@#%/:])(M+)|(?<=t+)(M+)";
            if (Regex.Match(date, checkMonth).Success)
            {
                date = Regex.Replace(date, checkMonth, Regex.Match(date, checkMonth).Value.ToUpper());
            }
            if (Regex.Match(date, checkMinutes).Success)
            {
                date = Regex.Replace(date, checkMinutes, Regex.Match(date, checkMinutes).Value.ToLower());
            }
            return updatedTime.ToString(date, CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Function used to check whether the today function is the first function of the template or not
        /// </summary>
        /// <returns>Bool value</returns>
       public bool TodayFirst()
        {
            if(updatedTime == DateTime.MinValue)
            {
                throw new Exception("Today method must be called first");
            }
            else
            {
                return false;
            }
        }
    }
}
