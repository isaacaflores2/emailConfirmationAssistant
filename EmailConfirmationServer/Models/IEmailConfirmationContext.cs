﻿using EmailConfirmationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConfirmationServer.Models
{
    public interface IEmailConfirmationContext
    {
        IQueryable<Person> People { get; }
        IQueryable<Email> Emails { get; }
        
        
        void SaveChanges();
        Person FindPersonById(int id);
        IQueryable<Email> FindEmailById(int id);
        T Add<T>(T entity) where T : class;
    }
}
