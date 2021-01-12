using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace oblaka_chat
{
    public class ChatHub : Hub
    {
        public class Message
        {
            public string Name { get; set; }
            public string Text { get; set; }
        }
        public class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
        }
        public static int online = 0;
        public static List<User> Users = new List<User>();
        public static List<Message> Messages = new List<Message>();
        // Отправка сообщений
        public void Send(string name, string message)
        {
            Messages.Add(new Message { Name = name, Text = message});
            Clients.All.addMessage(name, message);
        }
        //Вход
        public void Sign(string uname, string psw)
        {
            if (Users.Exists(x => x.Name == uname && x.Password == psw))
            {
                foreach (User x in Users)
                {
                    if(x.Name == uname)
                    {
                        x.Id = Context.ConnectionId;
                        Clients.Caller.onSign(x.Id, x.Name, Users);
                        // Посылаем сообщение всем пользователям, кроме текущего
                        Clients.AllExcept(x.Id).newSign(x.Id, x.Name);
                        if (Messages.Count > 0)
                        {
                            Clients.Caller.oldMessage(Messages);
                        }
                        online++;
                        Clients.All.addMessage("Сервер", "Total useres connected: "+ online.ToString());
                        break;
                    }
                }
            }
            else
            {
                Clients.Caller.error("Упс");
            }
        }
        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = Users.FirstOrDefault(x => x.Id == Context.ConnectionId);
            if (item != null)
            {
                Users[Users.IndexOf(item)].Id = null;
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.Name);
                online--;
                Clients.All.addMessage("Сервер", "Total useres connected: " + online.ToString());
            }

            return base.OnDisconnected(stopCalled);
        }
        public void Reg(string uname, string psw)
        {
           if(uname.Length>0 && psw.Length > 0)
            {
                if(Users.Count > 0)
                {
                    if (Users.Exists(x => x.Name == uname))
                    {
                        Clients.Caller.error("Пользователь с таким логином уже есть");
                    }
                    else
                    {
                        Users.Add(new User { Name = uname, Password = psw });
                        Clients.Caller.onReg(uname);
                    }
                }
                else
                {
                    Users.Add(new User { Name = uname, Password = psw });
                    // Call the broadcastMessage method to update clients.
                    Clients.Caller.onReg(uname);
                }
            }
            else
            {
                Clients.Caller.error("Не оставляйте формы пустыми");
            }
        }
    }
}