using OfficeOpenXml;
using CrmArcheonzero.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrmArcheonzero.Services
{
    public class ExcelImportService
    {
        public (List<Client> clients, List<ClientTask> tasks, List<Interaction> interactions, List<Note> notes, List<string> errors) 
            ImportFromExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var clients = new List<Client>();
            var tasks = new List<ClientTask>();
            var interactions = new List<Interaction>();
            var notes = new List<Note>();
            var errors = new List<string>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // ... логика чтения листов ...
            }

            return (clients, tasks, interactions, notes, errors);
        }
    }
}