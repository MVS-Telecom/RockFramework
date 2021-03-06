using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Iridium360.Connect.Framework.Messaging.Legacy;
using Iridium360.Connect.Framework.Messaging;
using Iridium360.Connect.Framework.Models;
using Iridium360.Connect.Framework.Helpers;
using Iridium360.Connect.Framework.Util;
using ByskySubscriber = Bysky.Subscriber;
using Bysky.SDK;
using Message = Iridium360.Connect.Framework.Messaging.Message;
using Iridium360.Connect.Framework.Messaging.Storage;

namespace Iridium360.Connect.Framework.Tests.Messaging
{
    [TestClass]
    public class MessageTest
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestWeatherMO()
        {
            try
            {
                var b0 = "120C058D199984BC0700827FF4874CC87B0020F8477F2885BC0700827FF47F13007F0FE002FF3FFF4DEFD7002AF0FFCF9F057F0F0002FFE8FFE5D0970026F08FFE8F82DE6F0090E01FFDBFE9F5020005FED1FF33E2AE0040E01FFDBF2CE616C005FED1FF5500CC358002FCA3FF37BF5C03B8C03FFA7F56BC250020FCA3FF97465B0300C23FFA3F0B7EB60230817FF47F0058".ToByteArray();
                var m0 = WeatherMT.Unpack(b0, new InMemoryBuffer());

                var b = "120D050C37D4AA04063168394054D30028".ToByteArray();
                var m = WeatherMO.Unpack(b, new InMemoryBuffer());

                var lat = -27.112535;
                var lon = -109.293498;

                var message1 = WeatherMO.Create(ProtocolVersion.v1, lat, lon);
                var message2 = WeatherMO.Create(ProtocolVersion.v2__LocationFix, lat, lon);
                var message3 = WeatherMO.Create(ProtocolVersion.v3__WeatherExtension, lat, lon);
                var message4 = WeatherMO.Create(ProtocolVersion.v4__WeatherExtension, lat, lon, lat, lon, 6);
                var message5 = WeatherMO.Create(ProtocolVersion.v4__WeatherExtension, lat, lon);

                var bytes1 = message1.Pack()[0].Payload;
                var bytes2 = message2.Pack()[0].Payload;
                var bytes3 = message3.Pack()[0].Payload;
                var bytes4 = message4.Pack()[0].Payload;
                var bytes5 = message5.Pack()[0].Payload;

                var unpacked1 = (WeatherMO)WeatherMO.Unpack(bytes1, new InMemoryBuffer());
                var unpacked2 = (WeatherMO)WeatherMO.Unpack(bytes2, new InMemoryBuffer());
                var unpacked3 = (WeatherMO)WeatherMO.Unpack(bytes3, new InMemoryBuffer());
                var unpacked4 = (WeatherMO)WeatherMO.Unpack(bytes4, new InMemoryBuffer());
                var unpacked5 = (WeatherMO)WeatherMO.Unpack(bytes5, new InMemoryBuffer());
            }
            catch (Exception e)
            {
                Debugger.Break();
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestWeatherMT()
        {
            try
            {
                var weather = JsonConvert.DeserializeObject<i360PointForecast>(File.ReadAllText(@"C:\Users\Banana\Desktop\weather.json"));

                var message1 = WeatherMT.Create(ProtocolVersion.v1, weather);
                var message2 = WeatherMT.Create(ProtocolVersion.v2__LocationFix, weather);
                var message3 = WeatherMT.Create(ProtocolVersion.v3__WeatherExtension, weather);
                var message4 = WeatherMT.Create(ProtocolVersion.v4__WeatherExtension, weather);
                var message5 = WeatherMT.Create(ProtocolVersion.v4__WeatherExtension, weather, 7);

                var bytes1 = message1.Pack()[0].Payload;
                var bytes2 = message2.Pack()[0].Payload;
                var bytes3 = message3.Pack()[0].Payload;
                var bytes4 = message4.Pack()[0].Payload;
                var bytes5 = message5.Pack()[0].Payload;

                var unpacked1 = (WeatherMT)WeatherMT.Unpack(bytes1, new InMemoryBuffer());
                var unpacked2 = (WeatherMT)WeatherMT.Unpack(bytes2, new InMemoryBuffer());
                var unpacked3 = (WeatherMT)WeatherMT.Unpack(bytes3, new InMemoryBuffer());
                var unpacked4 = (WeatherMT)WeatherMT.Unpack(bytes4, new InMemoryBuffer());
                var unpacked5 = (WeatherMT)WeatherMT.Unpack(bytes5, new InMemoryBuffer());

                //var bytes1 = "1208058C9F98BEB30120867FF4FFEC3B1B0062F847FF2FB5B10230857FF47F125861074008FFE8FF8D165600C4F08FFE9F816107400CFFE8FFA516B600C4F08FFE6F022B6C01C8E01FFDBFD1C206C010FED1FFB32EAC054CE11FFDBFC4C29A8008FED1FF515085358608FCA3FF3758D70310C33FFA7F866DAD8339FCA3FF97D5D70818C23FFABF0AA2B00130847FF47F9A".ToByteArray();
                //var message1 = (WeatherMT)WeatherMT.Unpack(bytes1, new InMemoryBuffer());
            }
            catch (Exception e)
            {
                Debugger.Break();
                throw;
            }
        }



        [TestMethod]
        public void ResendMessagePartsTest()
        {
            try
            {
                byte[] indexes = new byte[] { 8, 9, 19, 20, 21, 78 };
                byte group = 159;

                var bytes = MessageAckMT.Create(ProtocolVersion.v3__WeatherExtension, group, indexes).Pack()[0].Payload;
                var message = Message.Unpack(bytes) as MessageAckMT;

                if (message.TargetGroup != group || !message.ResendIndexes.SequenceEqual(indexes))
                    Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }



        [TestMethod]
        public void MessageSentTest()
        {
            try
            {
                byte group = 35;

                var bytes = MessageSentMO.Create(ProtocolVersion.v3__WeatherExtension, group).Pack()[0].Payload;
                var message = Message.Unpack(bytes) as MessageSentMO;

                if (message.SentGroup != group)
                    Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }




        [TestMethod]
        public void FileTest()
        {
            try
            {
                File.Delete(PacketBufferHelper.GetBufferDbPath());
            }
            catch { }

            try
            {
                var file = File.OpenRead(@"C:\Users\Banana\Desktop\file.txt");
                var m = ChatMessageMT.Create(
                    ProtocolVersion.v3__WeatherExtension,
                    new Subscriber("79999740562", SubscriberNetwork.Mobile), null, null,
                    "hello image",
                    lat: 12,
                    lon: -56,
                    alt: 561,
                    file: file,
                    fileExtension: FileExtension.Jpg,
                    imageQuality: ImageQuality.Low);

                var packets = m.Pack();

                string s = packets[0].Payload.ToHexString();

                var buffer = new RealmPacketBuffer();
                ChatMessageMT message = null;

                foreach (var p in packets)
                    message = Message.Unpack(p.Payload, buffer) as ChatMessageMT;

                if (m.File.Length != message.File.Length)
                    Assert.Fail();

                var bytes = (message.File as MemoryStream).GetBuffer();
                File.WriteAllBytes(@"/Users/banana/Downloads/rockstar-media-test[unpacked].jpg", bytes);

            }
            catch (Exception e)
            {

            }
        }


        [TestMethod]
        public void Test222()
        {
            try
            {
                try
                {
                    File.Delete(PacketBufferHelper.GetBufferDbPath());
                }
                catch { }


                var buffer = new RealmPacketBuffer();


                var list = new List<byte[]>()
                {
                    "120808020B002F".ToByteArray(),
                };

                object message = null;

                foreach (var bytes in list)
                    message = Message.Unpack(bytes, buffer);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }

        }


        [TestMethod]
        public void Location__()
        {
            try
            {
                try
                {
                    File.Delete(PacketBufferHelper.GetBufferDbPath());
                }
                catch { }


                var buffer = new RealmPacketBuffer();
                string text = @"Shovel works with all types of exchanges. 
In this example, I've set up exchanges as direct exchanges which use routing keys. 
However, if you have fanout exchanges, those will work as well—but you won't indicate a routing key when setting up the shovel for the exchange.
To demonstarate how you can send messages, let's look at the shovels we created for the first exchange that we set up—compose-to-messages-rmq. 
This shovel connects to our Compose for RabbitMQ exchange compose-exchange with our messages-rmq exchange in our Messages for RabbitMQ instance. 
Ive also created two queues to send messages to: compose - queue in the Compose instance and messages - queue in the Messages for RabbitMQ instance. 
Since the shovel is sending messages to the two exchanges, the queues have been bound to the exchanges so that the messages are received and queued up.";

                var __a = ChatMessageMT.Create(ProtocolVersion.v3__WeatherExtension, Subscriber.Create("hello@world.com", SubscriberNetwork.Email), null, null, text);
                var __b = ChatMessageMO.Create(ProtocolVersion.v3__WeatherExtension, Subscriber.Create("79153925491", SubscriberNetwork.Mobile), null, null, text);

                var a = __a.Pack(56);
                var b = __b.Pack(56);

                var totalLength = a.Select(x => x.Payload.Length - 8).Sum();

                var m1_ = MessageMO.Unpack(b[1].Payload, buffer) as ChatMessageMO;
                var m2 = MessageMT.Unpack(a[1].Payload, buffer) as ChatMessageMT;
                var m1 = MessageMT.Unpack(a[0].Payload, buffer) as ChatMessageMT;
                var m2_ = MessageMO.Unpack(b[0].Payload, buffer) as ChatMessageMO;
                var m3 = MessageMT.Unpack(a[2].Payload, buffer) as ChatMessageMT;
                var m3_ = MessageMO.Unpack(b[2].Payload, buffer) as ChatMessageMO;

                if (!m3.Complete)
                    throw new Exception();

                if (m3.Text != text)
                    throw new Exception();

            }
            catch (Exception e)
            {

            }
        }




        [TestMethod]
        public async Task Bysky()
        {
            try
            {
                var buffer = new InMemoryBuffer();
                var bytes = "1204043F0D01286A1E27669F08768272FA8125E0B0BFCC6A0E14610A3481EFF952B4B05CFC2F53E7D2E013AEA013560F103035C55933596F5A4ED89BD6EB046DCD1C00CA".ToByteArray();
                var m = MessageMT.Unpack(bytes, buffer) as ChatMessageMT;
            }
            catch (Exception e)
            {

            }
        }


        [TestMethod]
        public void PackLegacy()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                var buffer = new InMemoryBuffer();

                //var bytes = "118B26010037393939393734303536323A46524F4D373939393937343035363228382E362E32302D31303A33352855544329293A20CFF0E8E2E5F2".ToByteArray();
                var bytes1 = "119607010037393939373136383933384062792D736B792E6E65743AD2E5F1F220322031323A3237".ToByteArray();

                var m1 = Legacy_MessageMT.Unpack(bytes1, buffer);
            }
            catch (Exception e)
            {

            }
        }

        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        /// <returns></returns>
        //        [TestMethod]
        //        public async Task Unpack__ANYTest()
        //        {
        //            try
        //            {
        //                var buffer = "120104324501288AA2087699F76D8272FA8125E0B0BFCC6A0E14E153F13C596F5A4E5C1A07804B0A536E8902932E27396F3FF0C08E0116".ToByteArray();
        //                //var buffer = Convert.FromBase64String("EgEEMUUBKGoeJ2afCHaCcvqBJeCwv8xqDhQhV/E8kVUcN1kVaEp0KZnchC9K/VuCOGdMiAB9");
        //                var _message = MessageMO.Unpack(buffer);

        //                if (_message is CheckMessagesMO)
        //                {

        //                }

        //                if (_message == null)
        //                    Assert.Fail();
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }

        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        /// <returns></returns>
        //        [TestMethod]
        //        public async Task Pack__WeatherMOTest()
        //        {
        //            try
        //            {
        //                string s1 = Encoding.UTF8.GetString("118310010037393939393734303536323A46524F4D37393939393734303536322832382E352E32302D30363A35302855544329293A2054657374".ToByteArray());
        //                string s2 = Encoding.UTF8.GetString("37393939393734303536323A46524F4D37393939393734303536322832382E352E32302D30363A35302855544329293A2054657374".ToByteArray());

        //                var message = WeatherMO.Create(12.12109375, -9.12109375);

        //                var buffer = message.Pack();
        //                string hex = buffer.ToHexString();

        //                var _message = MessageMO.Unpack(buffer) as WeatherMO;

        //                var __message = MessageMO.Unpack("0104173501288AA2087699F76D001000185EA872809B56258200D9".ToByteArray());

        //                if (_message == null)
        //                    Assert.Fail();
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }


        [TestMethod]
        public void ParseWeather()
        {
            try
            {
                var buffer = "1208058C19F1C2B10060817FF4FF321C0B0016F847FF37C017160002F08FFEDF9463016004FFE8FF193A160088F08FFE5F8A6301E002FFE8FF0810C3020005FED1FF1B342C008CE01FFD3F5BC3020011FED1FF4BF22B0098E01FFD5F01E3560080C03FFA7F9376050009FCA3FF67EC5600A8C33FFA7F496E050023FCA3FF3320DC0A0036F847FF6FCAAD0020827FF47F12".ToByteArray();
                var weather = MessageMT.Unpack(buffer);
            }
            catch (Exception e)
            {

            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public async Task Pack__WeatherMTTest()
        {
            //var r = await new HttpClient().GetAsync("http://demo.iridium360.ru/connect/weather?auth=d9fc554e3ad74919bf274e11bdfe07c3&lat=55.67578125&lon=37.255859375&interval=6");
            var r = await new HttpClient().GetAsync("http://demo.iridium360.ru/connect/weather?auth=d9fc554e3ad74919bf274e11bdfe07c3&lat=25.761&lon=-80.208&interval=6");
            var s = await r.Content.ReadAsStringAsync();


            try
            {
                var ffff = JsonConvert.DeserializeObject<i360WeatherForecast>(s);

                var fs = ffff.Forecasts.Select(x => new i360PointForecast()
                {
                    Lat = x.Lat,
                    Lon = x.Lon,
                    DayInfos = x.DayInfos,
                    TimeOffset = x.TimeOffset,
                    Forecasts = x.Forecasts.Select(z => new i360Forecast()
                    {
                        WindGust = z.WindGust,
                        CloudHeight = z.CloudHeight,
                        Visibility = z.Visibility,
                        Cloud = z.Cloud,
                        Date = z.Date,
                        Precipitation = z.Precipitation,
                        WindSpeed = z.WindSpeed,
                        WindDirection = z.WindDirection,
                        Temperature = z.Temperature,
                        SnowRisk = z.SnowRisk,
                        Pressure = z.Pressure

                    }).Take(16).ToList()

                }).FirstOrDefault();

                var message = WeatherMT.Create(ProtocolVersion.v2__LocationFix, fs);
                var packets = message.Pack(33);

                var hex = packets[0].Payload.ToHexString();
                var m = Message.Unpack(packets[0].Payload) as WeatherMT;

            }
            catch (Exception e)
            {

            }
        }



        //[TestMethod]
        //public async Task Balance()
        //{
        //    try
        //    {
        //        var serial = "20975";
        //        var api = new i360ApiClient("<token>", serial);
        //        var result = await api.GetDeviceStatus();


        //        try
        //        {
        //            if (result?.Prepaid == null)
        //            {
        //                Debugger.Break();
        //                //continue;
        //            }

        //            var time = new DateTime(2020, 11, 17, 23, 58, 45, 0, DateTimeKind.Utc); //DateTime.UtcNow.AddMinutes(25);//.Date;
        //            var start = result.Prepaid.MonthlyBegin?.Date;
        //            var end = result.Prepaid.MonthlyNext?.Date;
        //            var a = result.Prepaid.Balance;
        //            var b = result.Prepaid.Units;
        //            var c = result.Prepaid.Usages;

        //            var p = BalanceMT.Create(ProtocolVersion.v3__WeatherExtension, time, start, end, a, b, c);
        //            var packets = p.Pack();
        //            var hex = packets[0].Payload.ToHexString();
        //            var p2 = (BalanceMT)BalanceMT.Unpack(packets[0].Payload);

        //            if (Math.Abs((time - p2.Time).TotalMinutes) > TimeSpan.FromMinutes(15).TotalMinutes)
        //                Assert.Fail();

        //            if (start != p2.MonthlyBegin)
        //                Assert.Fail();

        //            if (end != p2.MonthlyNext)
        //                Assert.Fail();

        //            if (a != p2.Balance)
        //                Assert.Fail();

        //            if (b != p2.Units)
        //                Assert.Fail();

        //            if (c != p2.Usages)
        //                Assert.Fail();

        //            //Debugger.Log(0, null, $"{line} OK\n");
        //        }
        //        catch (Exception e)
        //        {
        //            Debugger.Break();
        //        }
        //    }
        //    catch (Exception e2)
        //    {
        //        Debugger.Break();
        //    }
        //}



        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        [TestMethod]
        //        public void Pack__ChatMessageMOTest2()
        //        {
        //            try
        //            {
        //                ChatMessageMO emo1 = ChatMessageMO.Create(new Subscriber("79999740562", SubscriberNetwork.Mobile)
        //                        , 123
        //                        , 56789
        //                        , "part 1"
        //                        , -33.425238, -71.700784
        //                        , null
        //                        );

        //                ChatMessageMO emo2 = ChatMessageMO.Create(null
        //                        , null
        //                        , null
        //                        , "part 2"
        //                        , null
        //                        , null
        //                        );

        //                ChatMessageMO emo3 = ChatMessageMO.Create(null
        //                        , null
        //                        , null
        //                        , "part 3"
        //                        , null
        //                        , null
        //                        );


        //                var b1 = emo1.Pack();
        //                var b2 = emo2.Pack();
        //                var b3 = emo3.Pack();

        //                var __emo1 = (ChatMessageMO)MessageMO.Unpack(b1);
        //                var __emo2 = (ChatMessageMO)MessageMO.Unpack(b2);
        //                var __emo3 = (ChatMessageMO)MessageMO.Unpack(b3);
        //            }
        //            catch (Exception e)
        //            {

        //            }

        //        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Pack__ChatMessageMTTest2()
        {
            ChatMessageMT emt = ChatMessageMT.Create(
                ProtocolVersion.v3__WeatherExtension,
                new Subscriber("hello@world", SubscriberNetwork.Email)
                    , 123
                    , null
                    , "The markdown document and any attachments, such as images, will then be sent to your email address"
                    , null
                    );


            var packets = emt.Pack();
            var emt2 = __Unpack(packets) as ChatMessageMT;
        }



        Message __Unpack(List<Packet> packets)
        {
            var buffer = new InMemoryBuffer();
            Message message = null;

            foreach (var p in packets)
                message = Message.Unpack(p.Payload, buffer);

            return message;
        }



        //        /// <summary>
        //        ///  сообщения
        //        /// </summary>
        //        [TestMethod]
        //        public void Pack__ChatMessageMOTest()
        //        {
        //            for (ushort i = 0; i < 100; i++)
        //            {
        //                ushort? nullable;
        //                int? nullable4;
        //                ushort? nullable1;
        //                int? nullable5;
        //                int? nullable6;
        //                if ((i % 2) != 0)
        //                {
        //                    nullable1 = new ushort?((ushort)i);
        //                }
        //                else
        //                {
        //                    nullable = null;
        //                    nullable1 = nullable;
        //                }
        //                ChatMessageMO emo = ChatMessageMO.Create(new Subscriber("hello@world", SubscriberNetwork.Email)
        //                    , (ushort)(i * i)
        //                    , i
        //                    , (string)new string('t', i)
        //                    );
        //                var emo2 = (ChatMessageMO)MessageMO.Unpack(emo.Pack());
        //                if (emo.Subscriber != emo2.Subscriber)
        //                {
        //                    throw new InvalidOperationException("ChatId");
        //                }
        //                nullable = emo.Conversation;
        //                if (nullable.HasValue)
        //                {
        //                    nullable5 = new int?(nullable.GetValueOrDefault());
        //                }
        //                else
        //                {
        //                    nullable4 = null;
        //                    nullable5 = nullable4;
        //                }
        //                int? nullable2 = nullable5;
        //                nullable = emo2.Conversation;
        //                if (nullable.HasValue)
        //                {
        //                    nullable6 = new int?(nullable.GetValueOrDefault());
        //                }
        //                else
        //                {
        //                    nullable4 = null;
        //                    nullable6 = nullable4;
        //                }
        //                int? nullable3 = nullable6;
        //                if (!((nullable2.GetValueOrDefault() == nullable3.GetValueOrDefault()) & (nullable2.HasValue == nullable3.HasValue)))
        //                {
        //                    throw new InvalidOperationException("Conversation");
        //                }
        //                if (emo.Text != (emo2.Text ?? ""))
        //                {
        //                    throw new InvalidOperationException("Text");
        //                }
        //                if (emo.Subject != (emo2.Subject ?? ""))
        //                {
        //                    throw new InvalidOperationException("Subject");
        //                }
        //            }
        //        }

        //        [TestMethod]
        //        public void Pack__EmptyMO()
        //        {
        //            for (int i = 0; i < 0x3e8; i++)
        //            {
        //                string greeting = (string)new string('x', i);
        //                EmptyMO ymo = EmptyMO.Create(greeting);
        //                byte[] buffer = ymo.Pack();
        //                EmptyMO ymo2 = MessageMO.Unpack(buffer) as EmptyMO;
        //                if (ymo2.Length != ymo.Length)
        //                {
        //                    throw new InvalidOperationException("lenght");
        //                }
        //                if (ymo2.Greeting != greeting)
        //                {
        //                    throw new InvalidOperationException("gretings");
        //                }
        //            }
        //        }


        //        [TestMethod]
        //        public void Pack_VK()
        //        {
        //            var str = @"the new version of a good one for you can be fun and you should get a free game for a bit but it’s";


        //            var message = FreeTextMO.Create(str);


        //            var buffer = message.Pack();

        //            var _message = MessageMO.Unpack(buffer) as FreeTextMO;

        //            if (message.Text != _message.Text)
        //                Assert.Fail();
        //        }


        //        [TestMethod]
        //        public void Pack__FreeTextMO()
        //        {
        //            FreeTextMO tmo = FreeTextMO.Create("\nApple представила iPhone SE — первый смартфон компании в 2020 году. Аппарат стал преемником одноименного девайса, выпущенного в 2016 году. Об этом \x00abЛенте.ру\x00bb сообщил представитель компании.\nСмартфон получил внешний вид, схожий с дизайном iPhone 8: 4,7-дюймовый Retina IPS-экран, кнопка Home с дактилоскопическим сенсором Touch ID, крупные горизонтальные рамки на передней панели. На задней панели девайса расположена одинарная камера разрешением 12 мегапикселей. SE имеет чип Apple A13, как у iPhone 11 и 64 гигабайт встроенной памяти в минимальной комплектации.\n\x00abДешевый\x00bb iPhone имеет стеклянный корпус с рамкой из металла, который защищен от воды и пыли. Доступны белый, черный и красный цвета корпуса. У белой версии смартфона фронтальная панель все равно черная. Устройство базируется на актуальной iOS 13, имеет NFC с поддержкой бесконтактной оплаты Apple Pay, несъемный аккумулятор, емкость которого не раскрывается.\nСтоимость базовой версии iPhone SE составит 40 тысяч рублей — это самый дешевый актуальный смартфон компании. Продажи в России начнутся 24 апреля.\n");
        //            FreeTextMO tmo2 = (FreeTextMO)MessageMO.Unpack(tmo.Pack());
        //            if (!tmo.Text.Equals(tmo2.Text))
        //            {
        //                throw new InvalidOperationException();
        //            }
        //            tmo = FreeTextMO.Create("Смартфон получил внешний вид");
        //            tmo2 = (FreeTextMO)MessageMO.Unpack(tmo.Pack());
        //            if (!tmo.Text.Equals(tmo2.Text))
        //            {
        //                throw new InvalidOperationException();
        //            }
        //            for (int i = 1; i < 100; i++)
        //            {
        //                tmo = FreeTextMO.Create((string)new string('x', i));
        //                byte[] buffer = tmo.Pack();
        //                tmo2 = (FreeTextMO)MessageMO.Unpack(buffer);
        //                if (!tmo.Text.Equals(tmo2.Text))
        //                {
        //                    throw new InvalidOperationException();
        //                }
        //            }
        //        }


        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        [TestMethod]
        //        public void ParseFromDevice()
        //        {
        //            var bug = MessageMO.Unpack(StringToByteArray("0104150501288260C779D9177AA920D3A71BABC0302A0E0005")) as ChatMessageMO;

        //            Debug.WriteLine($"bug => `{bug.Text}`");

        //            var nobug = MessageMO.Unpack(StringToByteArray("0104180501288260C779D9177AA920D3A71BABC0D036C9A238090052")) as ChatMessageMO;
        //            Debug.WriteLine($"nobug => `{nobug.Text}`");


        //            var tests = new string[] {
        //                @"
        //01 04 1C 05 01 28 82 60 C7 79 D9 17 7A A9 20 D3
        //A7 1B AB C0 20 5B D6 42 38 43 53 41 4D 02 00 95",
        //                @"
        //01 04 1E 05 01 28 82 60 C7 79 D9 17 7A A9 20 D3
        //A7 1B AB C0 D0 36 89 66 59 0B E1 0C 4D 05 35 09
        //00 7C",
        //                @"
        //01 04 16 05 01 28 82 60 C7 79 D9 17 7A A9 20 D3
        //A7 1B AB C0 F0 42 95 93 00 F8",
        //                @"01 04 16 05 01 28 82 60 C7 79 D9 17 7A A9 20 D3
        //A7 1B AB C0 F0 5A 95 03 00 80",
        //            };


        //            foreach (var test in tests)
        //            {
        //                var message = MessageMO.Unpack(StringToByteArray(test));

        //                if (message is ChatMessageMO chatMessage)
        //                {
        //                    Debug.WriteLine($"Messge is `{message.GetType()}`");

        //                    Debug.WriteLine($"\t, chat:`{chatMessage.Subscriber}`");
        //                    Debug.WriteLine($"\t, id:`{chatMessage.Id}`");
        //                    Debug.WriteLine($"\t, converssation:`{chatMessage.Conversation}`");
        //                    Debug.WriteLine($"\t, subject:`{chatMessage.Subject}`");
        //                    Debug.WriteLine($"\t, text:`{chatMessage.Text}`");
        //                }

        //            }

        //        }




        /// <summary>
        ///     
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            while (hex.Contains(" ") || hex.Contains("\r") || hex.Contains("\n"))
            {
                hex = hex.Replace(" ", "");
                hex = hex.Replace("\r", "");
                hex = hex.Replace("\n", "");
            }

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        private static byte[] Merge(List<byte[]> arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }


    }
}