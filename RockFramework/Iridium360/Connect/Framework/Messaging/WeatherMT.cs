﻿using Iridium360.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Iridium360.Connect.Framework.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public class WeatherMT : MessageMT
    {
        /// <summary>
        /// 
        /// </summary>
        public List<i360PointForecast> Forecasts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override MessageType Type => MessageType.Weather;



        /// <summary>
        /// Получить кол-во битов необходимых для хранения значений в интервале [0..<paramref name="max"/>]
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetBits(int max)
        {
            int value = Math.Max(1, (int)Math.Ceiling(Math.Log(max, 2)));
            return value;
        }


        /// <summary>
        /// Получить кол-во битов необходимых для хранения значений в интервале [<paramref name="min"/>...<paramref name="max"/>]
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetBits(int min, int max)
        {
            return GetBits(max - min + 1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void pack(BinaryBitWriter writer)
        {
            var biterator = new Biterator();

            biterator.WriteInt(Forecasts.Count, GetBits(max: 4));

            foreach (var f in Forecasts)
            {
                biterator.WriteFloat((float)f.Lat, true, 11);
                biterator.WriteFloat((float)f.Lon, true, 11);

                ///->
                
                biterator.WriteInt(f.TimeOffset, GetBits(min: -12, max: 14));
                biterator.WriteUInt((uint)f.DayInfos.Count, GetBits(max: 5));

                foreach (var d in f.DayInfos)
                {
                    biterator.WriteUInt((uint)d.Day, GetBits(min: 0, max: 14600));
                    biterator.WriteUInt((uint)d.Forecasts.Count, GetBits(max: 8));

                    foreach (var ff in d.Forecasts)
                    {
                        biterator.WriteUInt((uint)ff.HourOffset, GetBits(min: 0, max: 24));
                        biterator.WriteInt(ff.Temperature, GetBits(min: -70, max: 50));

                        int? _cloud = ff.Cloud;

                        if (_cloud != null)
                            _cloud = (int)Math.Round(_cloud.Value / 20d);

                        biterator.WriteUInt((uint)(_cloud ?? 21), GetBits(min: 0, max: 20 + 1));

                        double? _precipitation = ff.Precipitation;

                        if (_precipitation.Value >= 0.05 && _precipitation.Value <= 0.1)
                            _precipitation = 1;
                        else
                            _precipitation = (int)Math.Round(_precipitation.Value * 4d);

                        biterator.WriteUInt((uint)(_precipitation ?? 241), GetBits(min: 0, max: 240 + 1));

                        ///->

                        int? _windDirection = ff.WindDirection;

                        if (_windDirection != null)
                            _windDirection = (int)(_windDirection.Value / 45d);

                        biterator.WriteUInt((uint)(_windDirection ?? 9), GetBits(min: 0, max: 8 + 1));

                        ///->

                        double? _windSpeed = ff.WindSpeed;

                        if (_windSpeed != null)
                            _windSpeed = (int)Math.Round(_windSpeed.Value * 2d);

                        biterator.WriteUInt((uint)(_windSpeed ?? 61), GetBits(min: 0, max: 60 + 1));

                        ///->

                        double? _pressure = ff.Pressure;

                        if (_pressure != null)
                            _pressure -= 580;

                        biterator.WriteUInt((uint)(_pressure ?? 222), GetBits(min: 0, max: 221 + 1));

                        ///->
                        biterator.WriteUInt((uint)(ff.SnowRisk != null ? (ff.SnowRisk.Value ? 1 : 0) : 2), GetBits(min: 0, max: 1 + 1));
                    }

                }
            }

            //var bytes = biterator.GetUsedBytes();
            //writer.Write(bytes, biterator.currentByte * 8 + biterator.currentBit);

            var bytes = biterator.GetUsedBytes();
            writer.Write(bytes);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        protected override void unpack(byte[] payload)
        {
            var biterator = new Biterator(payload);

            var size = biterator.ReadInt(GetBits(max: 4));
            var list = new List<i360PointForecast>();

            for (int i = 0; i < size; i++)
            {
                var f = new i360PointForecast();

                f.Lat = biterator.ReadFloat(true, 11);
                f.Lon = biterator.ReadFloat(true, 11);

                ///->
                
                f.TimeOffset = biterator.ReadInt(GetBits(min: -12, max: 14));

                var size2 = biterator.ReadUInt(GetBits(max: 5));
                var list2 = new List<i360DayInfo>();

                for (int j = 0; j < size2; j++)
                {
                    var d = new i360DayInfo();

                    d.Day = (int)biterator.ReadUInt(GetBits(min: 0, max: 14600));

                    var size3 = biterator.ReadUInt(GetBits(max: 8));
                    var list3 = new List<i360Forecast>();

                    for (int k = 0; k < size3; k++)
                    {
                        var ff = new i360Forecast();

                        ff.HourOffset = (int)biterator.ReadUInt(GetBits(min: 0, max: 24));
                        ff.Temperature = biterator.ReadInt(GetBits(min: -70, max: 50));

                        ///->

                        int _cloud = (int)biterator.ReadUInt(GetBits(min: 0, max: 20 + 1));

                        if (_cloud == 21)
                            ff.Cloud = null;
                        else
                            ff.Cloud = _cloud * 20;

                        ///->
                        ///
                        int _precipitation = (int)biterator.ReadUInt(GetBits(min: 0, max: 240 + 1));

                        if (_precipitation == 241)
                            ff.Precipitation = null;
                        else
                            ff.Precipitation = _precipitation / 4d;

                        ///->

                        int _windDirection = (int)biterator.ReadUInt(GetBits(min: 0, max: 8 + 1));

                        if (_windDirection == 9)
                            ff.WindDirection = null;
                        else
                            ff.WindDirection = (int)Math.Round(_windDirection * 45d);

                        ///-->

                        int _windSpeed = (int)biterator.ReadUInt(GetBits(min: 0, max: 60 + 1));

                        if (_windSpeed == 61)
                            ff.WindSpeed = null;
                        else
                            ff.WindSpeed = _windSpeed / 2d;

                        ///->

                        int _pressure = (int)biterator.ReadUInt(GetBits(min: 0, max: 221 + 1));

                        if (_pressure == 222)
                            ff.Pressure = null;
                        else
                            ff.Pressure = _pressure + 580;

                        ///->

                        int? _snowRisk = (int)biterator.ReadUInt(GetBits(min: 0, max: 1 + 1));

                        if (_snowRisk == 2)
                            ff.SnowRisk = null;
                        else
                            ff.SnowRisk = _snowRisk == 1 ? true : false;

                        ///->

                        list3.Add(ff);
                    }

                    d.Forecasts = list3;
                    list2.Add(d);

                }

                f.DayInfos = list2;
                list.Add(f);
            }


            Forecasts = list;
        }


        /// <summary>
        /// 
        /// </summary>
        private WeatherMT() { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="conversation"></param>
        /// <param name="text"></param>
        /// <param name="subject"></param>
        public static WeatherMT Create(List<i360PointForecast> forecasts)
        {
            WeatherMT weather = new WeatherMT();

            weather.Forecasts = forecasts;
            // --->
            return weather;
        }
    }

}

