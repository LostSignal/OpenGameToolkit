//-----------------------------------------------------------------------
// <copyright file="BetterStringBuilder.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace OGT
{
    public enum IntFormat
    {
        Plain,
        ThousandsSeperated,
        Abbreviated = 100,
    }

    public enum FloatFormat
    {
        Plain,
        ThousandsSeperated,
        HeightFeetInches,
    }

    public enum DecimalPlaces
    {
        Zero,
        One,
        Two,
        Three,
    }

    public struct BetterStringBuilder
    {
        private const long Billion = 1000000000;
        private const long Million = 1000000;
        private const long Thousand = 1000;

        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private static readonly char[] Digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static readonly char[] CharBuffer = new char[512];
        private static int currentLength;

        private static string thousandsSeperator = ",";
        private static string decimalSeperator = ".";

        public char[] CurrentCharBuffer => CharBuffer;

        public int CurrentCharBufferLength => currentLength;

        public static BetterStringBuilder New()
        {
            currentLength = 0;
            return default;
        }

        public static void SetThousandsSeperator(string seperator)
        {
            thousandsSeperator = seperator;
        }

        public static void SetDecimalPointSeperator(string seperator)
        {
            decimalSeperator = seperator;
        }

        public BetterStringBuilder Append(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    CharBuffer[currentLength++] = value[i];
                }
            }

            return this;
        }

        public BetterStringBuilder AppendLine(string value)
        {
            return this.Append(value).AppendLine();
        }

        public BetterStringBuilder AppendLine()
        {
            return this.Append("\n");
        }

        public BetterStringBuilder Append(char value)
        {
            CharBuffer[currentLength++] = value;
            return this;
        }

        public BetterStringBuilder Append(int value, IntFormat format = IntFormat.Plain)
        {
            return this.Append((long)value, format);
        }

        public BetterStringBuilder AppendTwoDigitNumber(int value)
        {
            if (value >= 100)
            {
                Logger.LogWarning("AppendTwoDigitNumber was given a number greater than 2 digits.");
            }

            if (value >= 10)
            {
                return this.Append((long)value, IntFormat.Plain);
            }
            else
            {
                return this.Append(0).Append((long)value, IntFormat.Plain);
            }
        }

        public BetterStringBuilder Append(long value, IntFormat format = IntFormat.Plain)
        {
            switch (format)
            {
                case IntFormat.Plain:
                    return this.AppendLong(value, false);

                case IntFormat.ThousandsSeperated:
                    return this.AppendLong(value, true);

                case IntFormat.Abbreviated:
                    return this.AppendAbbreviated(value);

                default:
                    Logger.LogErrorFormat("Found Unknown IntFormat {0}", format);
                    return this.AppendLong(value, false);
            }
        }

        public BetterStringBuilder Append(float value, int decimalPlaces, FloatFormat format = FloatFormat.Plain)
        {
            BetterStringBuilder current = this;

            // Special case for showing Height/Inches
            if (format == FloatFormat.HeightFeetInches)
            {
                double feetAsDouble = value / 0.3048;
                long feetAsLong = (long)feetAsDouble;
                double inches = (feetAsDouble - feetAsLong) * 12.0;
                long inchesLeft = (long)inches;
                double inchesRight = inches - inchesLeft;

                current = current
                    .Append(feetAsLong)
                    .Append("' ")
                    .Append((int)inches);

                current = AppendDecimalPlaces(current, inchesRight, decimalPlaces);
                current = current.Append("\"");
            }
            else
            {
                long leftSide = (long)value;
                double rightSide = value - leftSide;

                current = current.AppendLong(leftSide, format == FloatFormat.ThousandsSeperated);
                current = AppendDecimalPlaces(current, rightSide, decimalPlaces);
            }

            return current;

            static BetterStringBuilder AppendDecimalPlaces(BetterStringBuilder current, double value, int decimalPlaces)
            {
                if (decimalPlaces > 0)
                {
                    current = current.Append(decimalSeperator);

                    int factor = 1;
                    for (int i = 0; i < decimalPlaces; i++)
                    {
                        factor *= 10;
                        current = current.Append((int)(value * factor) % 10);
                    }
                }

                return current;
            }
        }

        public override string ToString()
        {
            return new string(CharBuffer, 0, currentLength);
        }

        private BetterStringBuilder AppendLong(long value, bool showThousandsSeperator)
        {
            string seperator = showThousandsSeperator ? thousandsSeperator : string.Empty;

            if (value < 0)
            {
                CharBuffer[currentLength++] = '-';
                value *= -1;
            }

            int digitCount = 1;
            ulong divisor = 10;
            ulong newValue = (ulong)value;

            while (newValue / divisor != 0)
            {
                divisor *= 10;
                digitCount++;
            }

            for (int i = 0; i < digitCount; i++)
            {
                divisor /= 10;
                CharBuffer[currentLength++] = Digits[(newValue / divisor) % 10];

                //// TODO [bgish]: Pretty sure this can be simplified by using (digitCount - i) % 3
                if (divisor == 1000 ||
                    divisor == 1000000 ||
                    divisor == 1000000000 ||
                    divisor == 1000000000000 ||
                    divisor == 1000000000000000 ||
                    divisor == 1000000000000000000)
                {
                    this.Append(seperator);
                }
            }

            return this;
        }

        private BetterStringBuilder AppendAbbreviated(long value)
        {
            long number = value;
            long remainder = 0;
            string postfix = string.Empty;

            if (value >= Billion)
            {
                number = value / Billion;
                remainder = (value - (number * Billion)) / (Billion / 10);
                postfix = "B";
            }
            else if (value >= Million)
            {
                number = value / Million;
                remainder = (value - (number * Million)) / (Million / 10);
                postfix = "M";
            }
            else if (value >= Thousand)
            {
                number = value / Thousand;
                remainder = (value - (number * Thousand)) / (Thousand / 10);
                postfix = "K";
            }

            if (remainder != 0)
            {
                return BetterStringBuilder.New()
                    .Append(number)
                    .Append(decimalSeperator)
                    .Append(remainder)
                    .Append(postfix);
            }
            else
            {
                return BetterStringBuilder.New().Append(number).Append(postfix);
            }
        }
    }
}
