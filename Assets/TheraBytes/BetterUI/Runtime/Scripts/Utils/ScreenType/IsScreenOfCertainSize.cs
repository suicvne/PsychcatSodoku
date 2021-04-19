﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [Serializable]
    public class IsScreenOfCertainSize : IScreenTypeCheck
    {
        public enum ScreenMeasure
        {
            Width,
            Height,
            Diagonal,
        }

        public enum UnitType
        {
            Inches,
            Centimeters,
        }
        
        [SerializeField]
        ScreenMeasure measureType = ScreenMeasure.Height;

        [SerializeField]
        UnitType unitType;

        [SerializeField]
        float minSizeInInches = 4.7f;

        [SerializeField]
        float maxSizeInInches = 7.6f;

        public ScreenMeasure MeasureType { get { return measureType; } set { measureType = value; } }

        public UnitType Units { get { return unitType; } set { unitType = value; } }

        public float MinSize
        {
            get { return (unitType == UnitType.Centimeters) ? 2.54f * minSizeInInches : minSizeInInches; }
            set { minSizeInInches = (unitType == UnitType.Centimeters) ? value / 2.54f : value; }
        }

        public float MaxSize
        {
            get { return (unitType == UnitType.Centimeters) ? 2.54f * maxSizeInInches : maxSizeInInches; }
            set { maxSizeInInches = (unitType == UnitType.Centimeters) ? value / 2.54f : value; }
        }

        [SerializeField]
        bool isActive;
        public bool IsActive { get { return isActive; } set { isActive = value; } }


        public bool IsScreenType()
        {
            Vector2 res = ResolutionMonitor.CurrentResolution;
            float dpi = ResolutionMonitor.CurrentDpi;

            float size = 0;
            switch (measureType)
            {
                case ScreenMeasure.Width:
                    size = res.x / dpi;
                    break;
                case ScreenMeasure.Height:
                    size = res.y / dpi;
                    break;
                case ScreenMeasure.Diagonal:
                    size = Mathf.Sqrt(res.x * res.x + res.y * res.y) / dpi;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return size >= minSizeInInches
                && size < maxSizeInInches;
        }
    }
}
