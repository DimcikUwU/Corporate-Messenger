using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace DBProject
{
    internal class Program
    {
        SqlConnection connection = Database.GetInstance();

        static void Main(string[] args)
        {
            bool appRunning = true;
            while (appRunning)
            {

                Console.WriteLine("Vyberte možnost:");
                Console.WriteLine("1. Register a new user");
                Console.WriteLine("2. User login");
                Console.WriteLine("3. Send message");
                Console.WriteLine("4. Seznam doručených zpráv");
                Console.WriteLine("5. Seznam odeslaných zpráv");
                Console.WriteLine("6. Smazat zprávu");
                Console.WriteLine("7. Konec aplikace");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        RegisterNewUser();
                        break;
                    case "2":
                        LogIn();
                        break;
                    case "3":
                        SendMessage();
                        break;
                    case "4":
                        ViewRM();
                        break;
                    case "5":
                        ViewSentMessages();
                        break;
                    
                    case "7":
                        appRunning = false;
                        break;
                    default:
                        Console.WriteLine("Neplatná volba, zkuste to znovu.");
                        break;
                }
            }
        }

        static void RegisterNewUser()
        {
            Console.WriteLine("Registrace nového uživatele");
            Console.Write("Zadejte uživatelské jméno: ");
            string username = Console.ReadLine();

            Console.Write("Zadejte heslo: ");
            string password = Console.ReadLine();


            using (SqlConnection connection = Database.GetInstance())
            {
                try
                {

                    string checkUsrnameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    SqlCommand checkUsrnameCmd = new SqlCommand(checkUsrnameQuery, connection);
                    checkUsrnameCmd.Parameters.AddWithValue("@Username", username);
                    int usrnameCount = (int)checkUsrnameCmd.ExecuteScalar();


                    if (usrnameCount > 0)
                    {
                        Console.WriteLine("Uživatelské jméno již existuje, zkuste zvolit jiné.");
                        return;
                    }


                    string registerQuery = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                    SqlCommand registerCmd = new SqlCommand(registerQuery, connection);
                    registerCmd.Parameters.AddWithValue("@Username", username);
                    registerCmd.Parameters.AddWithValue("@Password", password);
                    registerCmd.ExecuteNonQuery();

                    Console.WriteLine("Registrace proběhla úspěšně.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong :(");

                }
            }
        }

        static int LogIn()
        {
            Console.WriteLine("Přihlášení uživatele");
            Console.WriteLine("Zadejte uživatelské jméno: ");
            string usernameL = Console.ReadLine();
            Console.WriteLine("Zadejte heslo:");
            string passwordL = Console.ReadLine();

            using (SqlConnection connection = Database.GetInstance())
            {

                try
                {


                    string loginQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";
                    SqlCommand loginCmd = new SqlCommand(loginQuery, connection);
                    loginCmd.Parameters.AddWithValue("@Username", usernameL);
                    loginCmd.Parameters.AddWithValue("@Password", passwordL);
                    int userCount1 = (int)loginCmd.ExecuteScalar();

                    if (userCount1 > 0)
                    {
                        Console.WriteLine("Přihlášení úspěšné.");
                        string getUserIdQuery = "SELECT ID FROM Users WHERE Username = @Username AND Password = @Password";
                        SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, connection);
                        getUserIdCmd.Parameters.AddWithValue("@Username", usernameL);
                        getUserIdCmd.Parameters.AddWithValue("@Password", passwordL);
                        int userId = (int)getUserIdCmd.ExecuteScalar();

                        Console.WriteLine("Vítejte, uživateli číslo {0}", userId);
                        return userId;
                    }

                    else
                    {
                        Console.WriteLine("Přihlášení se nezdařilo.");
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong :(");
                    return -1;
                }




            }

        }

        static void SendMessage()
        {
            Console.WriteLine("Odeslat zprávu");

            int userId = LogIn();
            if (userId == -1)
            {
                Console.WriteLine("Pro odeslání zprávy se musíte přihlásit.");
                return;
            }

            Console.Write("Zadejte uživatelské jméno příjemce: ");
            string recipientUsername = Console.ReadLine();

            Console.Write("Zadejte téma zprávy: ");
            string subject = Console.ReadLine();

            Console.Write("Zadejte text zprávy: ");
            string text = Console.ReadLine();

            using (SqlConnection connection = Database.GetInstance())
            {
                try
                {
                    connection.Open();
                    string getRecipientIdQuery = "SELECT ID FROM Users WHERE Username = @Username";
                    SqlCommand getRecipientIdCmd = new SqlCommand(getRecipientIdQuery, connection);
                    getRecipientIdCmd.Parameters.AddWithValue("@Username", recipientUsername);
                    int recipientId = (int)getRecipientIdCmd.ExecuteScalar();


                    string sendMessageQuery = "INSERT INTO Messages (SenderID, RecipientID, Subject, Text, SentAt) VALUES (@SenderID, @RecipientID, @Subject, @Text, @SentAt)";
                    SqlCommand sendMessageCmd = new SqlCommand(sendMessageQuery, connection);
                    sendMessageCmd.Parameters.AddWithValue("@SenderID", userId);
                    sendMessageCmd.Parameters.AddWithValue("@RecipientID", recipientId);
                    sendMessageCmd.Parameters.AddWithValue("@Subject", subject);
                    sendMessageCmd.Parameters.AddWithValue("@Text", text);
                    sendMessageCmd.Parameters.AddWithValue("@SentAt", DateTime.Now);
                    sendMessageCmd.ExecuteNonQuery();

                    Console.WriteLine("Zpráva byla úspěšně odeslána.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Chyba při získávání ID příjemce: {0}", ex.Message);
                    return;
                }

            }
        }
        static void ViewRM()
        {
            Console.WriteLine("Seznam doručených zpráv");

            int userId = LogIn();
            if (userId == -1)
            {
                Console.WriteLine("Pro zobrazení doručených zpráv se musíte přihlásit.");
                return;
            }
            try
            {
                using (SqlConnection connection3 = Database.GetInstance())
                {
                    connection3.Open();

                    string getRMQuery = "SELECT Messages.MessageText, Messages.MessageSubject, Users.Username AS Sender Messages.SentAt FROM Messages JOIN Users ON Messages.SenderID = Users.ID WHERE RecipientID = @RecipientID";
                    SqlCommand getRMCmd = new SqlCommand(getRMQuery, connection3);
                    getRMCmd.Parameters.AddWithValue("@RecipientID", userId);

                    SqlDataReader reader = getRMCmd.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Nemáte žádné doručené zprávy.");
                        return;
                    }

                    Console.WriteLine("Doručené zprávy:");
                    while (reader.Read())
                    {
                        string messageText = reader.GetString(0);
                        string messageSubject = reader.GetString(1);
                        string sender = reader.GetString(2);
                        DateTime SentAt = reader.GetDateTime(3);


                        Console.WriteLine("Od: {0}", sender);
                        Console.WriteLine("Předmět: {0}", messageSubject);
                        Console.WriteLine("Datum odeslání: {0}", SentAt);
                        Console.WriteLine("Zpráva: {0}", messageText);
                        Console.WriteLine();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Při načítání doručených zpráv došlo k chybě: {0}", ex.Message);
            }

        }

        static void ViewSentMessages()
        {
            Console.WriteLine("Seznam odeslaných zpráv");

            int userId = LogIn();
            if (userId == -1)
            {
                Console.WriteLine("Pro zobrazení odeslaných zpráv se musíte přihlásit.");
                return;
            }

            using (SqlConnection connection = Database.GetInstance())
            {
            

                string getSentMessagesQuery = "SELECT Messages.MessageText, Users.Username AS Recipient FROM Messages JOIN Users ON Messages.RecipientID = Users.ID WHERE SenderID = @SenderID";
                SqlCommand getSentMessagesCommand = new SqlCommand(getSentMessagesQuery, connection);
                getSentMessagesCommand.Parameters.AddWithValue("@SenderID", userId);

                SqlDataReader reader = getSentMessagesCommand.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Nemáte žádné odeslané zprávy.");
                    return;
                }

                Console.WriteLine("Odeslané zprávy:");
                while (reader.Read())
                {
                    string messageText = reader.GetString(0);
                    string recipientUsername = reader.GetString(1);

                    Console.WriteLine($"Pro uživatele {recipientUsername}: {messageText}");
                }
            }

   
        }

        static void DeleteMessage()
        {
            Console.WriteLine("Smazat zprávu");

            

int userId = LogIn();
            if (userId == -1)
            {
                Console.WriteLine("Pro smazání zprávy se musíte přihlásit.");
                return;
            }

            Console.Write("Zadejte ID zprávy, kterou chcete smazat: ");
            int messageId;
            if (!int.TryParse(Console.ReadLine(), out messageId))
            {
                Console.WriteLine("Zadejte prosím číslo.");
                return;
            }

            using (SqlConnection connection = Database.GetInstance())
            {
                connection.Open();

                string checkMessageQuery = "SELECT COUNT(*) FROM Messages WHERE ID = @ID AND (SenderID = @UserID OR RecipientID = @UserID)";
                SqlCommand checkMessageCommand = new SqlCommand(checkMessageQuery, connection);
                checkMessageCommand.Parameters.AddWithValue("@ID", messageId);
                checkMessageCommand.Parameters.AddWithValue("@UserID", userId);
                int messageCount = (int)checkMessageCommand.ExecuteScalar();

                if (messageCount == 0)
                {
                    Console.WriteLine("Zprávu nelze smazat, protože neexistuje nebo na ni nemáte oprávnění.");
                    return;
                }

                string deleteMessageQuery = "DELETE FROM Messages WHERE ID = @ID";
                SqlCommand deleteMessageCommand = new SqlCommand(deleteMessageQuery, connection);
                deleteMessageCommand.Parameters.AddWithValue("@ID", messageId);
                deleteMessageCommand.ExecuteNonQuery();

                Console.WriteLine("Zpráva byla úspěšně smazána.");
            }

        }
    }
}
    
    
    