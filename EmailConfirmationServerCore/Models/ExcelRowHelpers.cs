﻿using EmailConfirmationServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmailConfirmationServerCore.Models
{
    public class ExcelRowHelpers
    {
        public static IEnumerable<Person> convertRowsToPeople(IEnumerable<PersonRow> rows)
        {
            var people = new List<Person>();
            int personId = 1;

            foreach (var row in rows)
            {
                if (row == null)
                    throw new FileFormatException(
                        "There was a null person row. Make sure to check the property names in the row model match the column names in the spreadhseet.");

                people.Add(convertRowToPerson(row));
            }

            return people; 
        }

        public static Person convertRowToPerson(PersonRow row)
        {
            Person person = new Person();
            person.Emails = new List<Email>();

            person.FirstName = row.FirstName;
            person.LastName = row.LastName;
            person.Emails.Add(new Email(row.Outlook));
            person.Emails.Add(new Email(row.StMartin));

            return person;
        }

        public static IEnumerable<PersonRow> convertToPersonRows(IEnumerable<Person> people)
        {
            var personRows = new List<PersonRow>();
            personRows.Add(
                new PersonRow
                {
                    FirstName = "Isaac",
                    LastName =  "Flores",
                    Outlook = "test@outlook.com",
                    StMartin = "test@stmartin.com",
                    SheetName = "Sheet1"
                }    
            );
            return personRows; 
        }

        public static PersonRow converToPersonRow(Person person)
        {
            var personRow = new PersonRow();

            return personRow; 
        }

    }
}
