/*
BSD 2-Clause License

Copyright (c) 2023, Gundorada Workshop 

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using Archipelago.Core.Util;
using System.Text;

namespace DC1AP.Mem
{
    /// <summary>
    /// Adapted from Dark Cloud Enhanced project Dayuppy.cs:DisplayMessageProcess()
    /// </summary>
    public class MessageFuncs
    {
        private const uint DunMsgAddr = 0x00998BB8;     //The address pointing to the text of the 10th dungeon message. 157 Byte array
        internal const uint DunMsgDurAddr = 0x01EA7694;  //How long to show the message
        private const uint DunMsgIdAddr = 0x01EA76B4;

        /// 
        /// <param name="message"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="displayTime"></param>
        /// <returns></returns>
        public static void DisplayMessageDungeon(string message, int height, int width, int displayTime)
        {
            byte[] customMessage = Encoding.GetEncoding("utf-8").GetBytes(message);
            byte[] dungeonMessage = Memory.ReadByteArray(DunMsgAddr, message.Length);

            byte[] outputMessage = new byte[customMessage.Length * 2];

            byte[] normalCharTable =
            {
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, //A-Z

                0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, //a-z

                //0     1     2     3     4     5     6     7     8     9
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,

                //'     =     "     !     ?     #     &     +     -     *     (     )    @     |     ^
                0x27, 0x3D, 0x22, 0x21, 0x3F, 0x23, 0x26, 0x2B, 0x2D, 0x2A, 0x28, 0x29, 0x40, 0x7C, 0x5E,

                //<     >     {    }     [     ]
                0x3C, 0x3E, 0x7B, 0x7D, 0x5B, 0x5D,

                //.    $     \n    SPC
                0x2E, 0x24, 0x0A, 0x20,

                //Cross     Circle
                0x8, 0x6,
            };

            byte[] dcCharTable =
            {
                0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, //A-Z

                0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
                0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, //a-z

                //0     1     2     3     4     5     6     7     8     9
                0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,

                //'     =     "     !     ?     #     &     +     -     *     (     )     @    |     ^
                0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x61, 0x62, 0x63, 0x64, 0xFF, //Just needed for detection, doesn't matter what this is

                //<     >     {    }     [      ]
                0x65, 0x66, 0x67, 0x68, 0x69, 0x6A,

                //.     $    \n    SPC
                0x6D, 0x6E, 0x00, 0x02,

                //Cross     Circle
                0x8, 0x6,
            };

            // Initialize outputMessage to 0xFD
            for (int i = 0; i < outputMessage.Length; i++)
            {
                outputMessage[i] = 0xFD;
            }

            // Initialize current dungeonMessage to nothing
            for (int i = 0; i < dungeonMessage.Length; i++)
            {
                dungeonMessage[i] = 0xFD;
            }

            /*
            for (int i = 0; i < dungeonMessage.Length; i += width) //Initialize Dungeon message with three lines.
            {
                //newLine
                dungeonMessage[i] = 0x00;
                dungeonMessage[i + 1] = 0xFF;
            }
            */

            Memory.WriteByteArray(DunMsgAddr, dungeonMessage);

            for (int i = 0; i < customMessage.Length; i++)
            {
                for (int t = 0; t < dcCharTable.Length; t++)
                {
                    if (customMessage[i] == normalCharTable[t])
                    {
                        if (normalCharTable[t] == 0x0A) //newLine
                        {
                            outputMessage[i * 2] = 0x00;
                            outputMessage[i * 2 + 1] = 0xFF;
                        }

                        else if (normalCharTable[t] == 0x20) //SPC
                        {
                            outputMessage[i * 2] = 0x02;
                            outputMessage[i * 2 + 1] = 0xFF;
                        }

                        else if (normalCharTable[t] == 0x5E) //^
                        {
                            if (customMessage[i + 1] == 0x57) //W
                            {
                                i++;  //Skip displaying the W
                                outputMessage[i * 2] = 0x01; //White
                                outputMessage[i * 2 + 1] = 0xFC;
                            }

                            else if (customMessage[i + 1] == 0x59) //Y
                            {
                                i++;
                                outputMessage[i * 2] = 0x02; //Yellow
                                outputMessage[i * 2 + 1] = 0xFC;
                            }

                            else if (customMessage[i + 1] == 0x42) //B
                            {
                                i++;
                                outputMessage[i * 2] = 0x03; //Blue
                                outputMessage[i * 2 + 1] = 0xFC;
                            }

                            else if (customMessage[i + 1] == 0x47) //G
                            {
                                i++;
                                outputMessage[i * 2] = 0x04; //Green
                                outputMessage[i * 2 + 1] = 0xFC;
                            }

                            //0x05 is a nasty brown color

                            else if (customMessage[i + 1] == 0x4F)
                            {
                                i++;
                                outputMessage[i * 2] = 0x06; //Orange
                                outputMessage[i * 2 + 1] = 0xFC;
                            }

                            //0x07 is a gray

                            else if (customMessage[i + 1] == 0x52)
                            {
                                i++;
                                outputMessage[i * 2] = 0xFF; //Red
                                outputMessage[i * 2 + 1] = 0xFC;
                            }
                        }

                        else
                            outputMessage[i * 2] = dcCharTable[t];
                    }
                }

                if (i == customMessage.Length - 1)
                {
                    uint aux;

                    aux = DunMsgAddr + (uint)outputMessage.Length;

                    Memory.WriteByte(aux, 1);
                    Memory.WriteByte(aux + 0x1, 255);
                }
            }


            //byte[] original10Message = { 52, 253, 66, 253, 63, 253, 76, 253, 63, 253, 2, 255, 67, 253, 77, 253, 2, 255, 72, 253, 73, 253, 2, 255, 77, 253, 67, 253, 65, 253, 72, 253, 2, 255, 73, 253, 64, 253, 2, 255, 71, 253, 73, 253, 72, 253, 77, 253, 78, 253, 63, 253, 76, 253, 77, 253, 2, 255, 0, 255, 73, 253, 72, 253, 2, 255, 78, 253, 66, 253, 67, 253, 77, 253, 2, 255, 64, 253, 70, 253, 73, 253, 73, 253, 76, 253, 109, 253, 2, 255, 57, 253, 73, 253, 79, 253, 2, 255, 61, 253, 59, 253, 72, 253, 2, 255, 79, 253, 77, 253, 63, 253, 2, 255, 83, 253, 73, 253, 79, 253, 76, 253, 2, 255, 0, 255, 63, 253, 77, 253, 61, 253, 59, 253, 74, 253, 63, 253, 2, 255, 77, 253, 69, 253, 67, 253, 70, 253, 70, 253, 109, 253, 0, 255, 3, 252, 87, 253, 44, 253, 63, 253, 59, 253, 80, 253, 63, 253, 2, 255, 36, 253, 79, 253, 72, 253, 65, 253, 63, 253, 73, 253, 72, 253, 87, 253, 0, 252, 2, 255, 59, 253, 80, 253, 59, 253, 67, 253, 70, 253, 59, 253, 60, 253, 70, 253, 63, 253, 88, 253, 1, 255 };
            int messageId = 10;

            Memory.Write(DunMsgAddr, 0xffffffff);       //Clear any display message
            Memory.WriteByteArray(DunMsgAddr, outputMessage); //Write our message string onto memory
            Memory.Write(DunMsgIdAddr, messageId);         //Display our custom message
            Memory.Write(DunMsgDurAddr, displayTime);  //Set our custom message duration
 
            //return outputMessage;
        }
    }
}