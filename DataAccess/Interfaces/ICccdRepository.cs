﻿using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ICccdRepository
    {
         Task<Cccd?> GetCccdByCode(string code);
        Task SaveCccd(Cccd cccd);

        Task<Cccd?> GetCccdById(int id);
    }
}
