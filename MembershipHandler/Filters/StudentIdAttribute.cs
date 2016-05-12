using MembershipHandler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembershipHandler.Filters
{
    public class StudentIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                // Because you dont need to have a student id
                return true;
            }
            if (value.GetType() == typeof(string))
            {
                // If its anything it has to be a valid id
                return IsStudentId((string)value);
            }
            // Otherwise false
            return false;
        }

        public static bool IsStudentId(string studentId)
        {
            if (studentId.StartsWith("a", StringComparison.InvariantCultureIgnoreCase))
            {
                if (studentId.Length == "a1234567".Length)
                {
                    string number = studentId.Substring(1, studentId.Length - 2);
                    int x;
                    if (int.TryParse(number, out x))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}