﻿using System.Collections.Generic;
using System.Linq;
using Combot.Databases;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Logging : Module
    {
        public override void Initialize()
        {
            //Bot.CommandReceivedEvent += HandleCommandEvent;
            Bot.IRC.ConnectEvent += AddServer;
            Bot.IRC.Message.ChannelMessageReceivedEvent += LogChannelMessage;
            Bot.IRC.Message.PrivateMessageReceivedEvent += LogPrivateMessage;
            Bot.IRC.Message.JoinChannelEvent += LogChannelJoin;
            Bot.IRC.Message.PartChannelEvent += LogChannelPart;
            Bot.IRC.Message.KickEvent += LogChannelKick;
            Bot.IRC.Message.QuitEvent += LogQuit;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
        }

        private void LogChannelMessage(object sender, ChannelMessage message)
        {
            AddChannel(message.Channel);
            AddNick(message.Sender.Nickname);
            Database database = new Database(Bot.ServerConfig.Database);
            string query = "INSERT INTO `channelmessages` SET " +
                           "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                           "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                           "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                           "`message` = {5}, " +
                           "`date_added` = {6}";
            database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, message.Channel, Bot.ServerConfig.Name, message.Sender.Nickname, message.Message, message.TimeStamp});
        }

        private void LogPrivateMessage(object sender, PrivateMessage message)
        {
            AddNick(message.Sender.Nickname);
            Database database = new Database(Bot.ServerConfig.Database);
            string query = "INSERT INTO `privatemessages` SET " +
                           "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                           "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2}), " +
                           "`message` = {3}, " +
                           "`date_added` = {4}";
            database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, message.Sender.Nickname, message.Message, message.TimeStamp});
        }

        private void LogChannelJoin(object sender, JoinChannelInfo info)
        {
            AddChannel(info.Channel);
            AddNick(info.Nick.Nickname);
            Database database = new Database(Bot.ServerConfig.Database);
            string query = "INSERT INTO `channeljoins` SET " +
                           "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                           "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                           "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                           "`date_added` = {5}";
            database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Nick.Nickname, info.TimeStamp});
        }

        private void LogChannelPart(object sender, PartChannelInfo info)
        {
            AddChannel(info.Channel);
            AddNick(info.Nick.Nickname);
            Database database = new Database(Bot.ServerConfig.Database);
            string query = "INSERT INTO `channelparts` SET " +
                           "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                           "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                           "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                           "`date_added` = {5}";
            database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Nick.Nickname, info.TimeStamp});
        }

        private void LogChannelKick(object sender, KickInfo info)
        {
            AddChannel(info.Channel);
            AddNick(info.Nick.Nickname);
            AddNick(info.KickedNick.Nickname);
            Database database = new Database(Bot.ServerConfig.Database);
            string query = "INSERT INTO `channelkicks` SET " +
                           "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                           "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                           "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                           "`kicked_nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {5} && `nickname` = {6}), " +
                           "`reason` = {7}, " +
                           "`date_added` = {8}";
            database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Nick.Nickname, Bot.ServerConfig.Name, info.KickedNick.Nickname, info.Reason, info.TimeStamp});
        }

        private void LogQuit(object sender, QuitInfo info)
        {
            AddNick(info.Nick.Nickname);
            Database database = new Database(Bot.ServerConfig.Database);
            string query = "INSERT INTO `quits` SET " +
                           "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                           "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2}), " +
                           "`message` = {3}, " +
                           "`date_added` = {4}";
            database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Nick.Nickname, info.Message, info.TimeStamp});
        }
    }
}
