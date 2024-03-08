using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationWithMimecastAPI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //In production env, we use Windows Task Scheduler to run Cron URL to perform some tasks periodically,
            //Here just show logic of Mimecast API Integration, so call Cron instance instead.
            var cronModel = new Cron();
            var emailModels = cronModel.GetAndSaveMimecastHeldEmails();

            //.......return all emails to front-end and display email list.
        }
    }
}
