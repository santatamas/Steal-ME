namespace StealME.Server.Core.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using StealME.Server.Data.DAL;

    public static class CommandLogic
    {
        public static List<Command> GetCommands()
        {
            var context = DataHandler.GetContext();
            var commandList = context.Command.ToList();
            foreach (var cmd in commandList)
            {
                context.Detach(cmd);
            }

            return commandList;
        }
        public static DateTime? GetLatestCommandDate()
        {
            var result = DataHandler.GetContext().Command.OrderByDescending(c => c.CreationDate).FirstOrDefault();
            if (result == null)
                return null;

            return result.CreationDate;
        }
        public static void DeleteCommand(Command c)
        {
            SMLogger.LogThis("CommandType deleted: " + c.CommandText);

            var context = DataHandler.GetContext();
            context.Attach(c);
            context.Command.DeleteObject(c);
            context.SaveChanges();
        }

        public static void AddCommand(Guid trackerId, string commandText)
        {
            StealMEEntities ctx = DataHandler.GetContext();
            ctx.AddToCommand(new Command
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.Now,
                    CommandText = commandText,
                    TrackerId = trackerId
                });
            ctx.SaveChanges();
            SMLogger.LogThis("CommandType added: " + commandText);
        }
    }
}
