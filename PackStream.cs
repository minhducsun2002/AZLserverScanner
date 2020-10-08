using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZLServerProtobuf
{
    public class PackStream //copied from original code
    {
        // Token: 0x0600420E RID: 16910 RVA: 0x0001D7CE File Offset: 0x0001B9CE
        public PackStream()
        {
            this.pktBuffer = new byte[8192];
            this.Length = 0;
            this.Seek = 0;
        }

        // Token: 0x0600420F RID: 16911 RVA: 0x0001D7F4 File Offset: 0x0001B9F4
        public PackStream(int length)
        {
            this.pktBuffer = new byte[length];
            this.Length = 0;
            this.Seek = 0;
        }

        // Token: 0x06004210 RID: 16912 RVA: 0x0001D816 File Offset: 0x0001BA16
        public PackStream(byte[] pktBuffer)
        {
            if (pktBuffer == null || pktBuffer.Length == 0)
            {
                throw new Exception("buf 不能为 null。");
            }

            this.Length = 0;
            this.Seek = 0;
            this.pktBuffer = pktBuffer;
        }

        // Token: 0x06004211 RID: 16913 RVA: 0x0016C2CC File Offset: 0x0016A4CC
        public override string ToString()
        {
            byte[] array = new byte[this.Length];
            Array.Copy(this.pktBuffer, array, this.Length);
            return Encoding.Unicode.GetString(array);
        }

        // Token: 0x17000585 RID: 1413
        // (get) Token: 0x06004212 RID: 16914 RVA: 0x0001D84C File Offset: 0x0001BA4C
        // (set) Token: 0x06004213 RID: 16915 RVA: 0x0001D854 File Offset: 0x0001BA54
        public int Seek { get; set; }

        // Token: 0x17000586 RID: 1414
        // (get) Token: 0x06004214 RID: 16916 RVA: 0x0001D85D File Offset: 0x0001BA5D
        // (set) Token: 0x06004215 RID: 16917 RVA: 0x0001D865 File Offset: 0x0001BA65
        public int Length { get; private set; }

        // Token: 0x06004216 RID: 16918 RVA: 0x0001D86E File Offset: 0x0001BA6E
        public void Reset()
        {
            this.Length = 0;
            this.Seek = 0;
        }

        // Token: 0x06004217 RID: 16919 RVA: 0x0016C304 File Offset: 0x0016A504
        private void Write(byte x)
        {
            this.pktBuffer[this.Seek++] = x;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x06004218 RID: 16920 RVA: 0x0016C348 File Offset: 0x0016A548
        private void Write(short x)
        {
            this.pktBuffer[this.Seek++] = (byte) (x >> 8);
            this.pktBuffer[this.Seek++] = (byte) x;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x06004219 RID: 16921 RVA: 0x0016C3A8 File Offset: 0x0016A5A8
        private void Write(int x)
        {
            this.pktBuffer[this.Seek++] = (byte) (x >> 24);
            this.pktBuffer[this.Seek++] = (byte) (x >> 16);
            this.pktBuffer[this.Seek++] = (byte) (x >> 8);
            this.pktBuffer[this.Seek++] = (byte) x;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x0600421A RID: 16922 RVA: 0x0016C444 File Offset: 0x0016A644
        private void Write(uint x)
        {
            while (((ulong) x & 18446744073709551488UL) != 0UL)
            {
                this.pktBuffer[this.Seek++] = (byte) ((x & 127u) | 128u);
                x >>= 7;
            }

            this.pktBuffer[this.Seek++] = (byte) x;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x0600421B RID: 16923 RVA: 0x0016C4C8 File Offset: 0x0016A6C8
        private void Write(long x)
        {
            this.pktBuffer[this.Seek++] = (byte) (x >> 56);
            this.pktBuffer[this.Seek++] = (byte) (x >> 48);
            this.pktBuffer[this.Seek++] = (byte) (x >> 40);
            this.pktBuffer[this.Seek++] = (byte) (x >> 32);
            this.pktBuffer[this.Seek++] = (byte) (x >> 24);
            this.pktBuffer[this.Seek++] = (byte) (x >> 16);
            this.pktBuffer[this.Seek++] = (byte) (x >> 8);
            this.pktBuffer[this.Seek++] = (byte) x;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x0600421C RID: 16924 RVA: 0x0016C5D8 File Offset: 0x0016A7D8
        private void Write(ulong x)
        {
            while ((x & 18446744073709551488UL) != 0UL)
            {
                this.pktBuffer[this.Seek++] = (byte) ((x & 127UL) | 128UL);
                x >>= 7;
            }

            this.pktBuffer[this.Seek++] = (byte) x;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x0600421D RID: 16925 RVA: 0x0016C65C File Offset: 0x0016A85C
        private void Write(byte[] bs, int offset, int len)
        {
            Array.Copy(bs, offset, this.pktBuffer, this.Seek, len);
            this.Seek += len;
            if (this.Seek > this.Length)
            {
                this.Length = this.Seek;
            }
        }

        // Token: 0x0600421E RID: 16926 RVA: 0x0001D87E File Offset: 0x0001BA7E
        public void WriteInt8(int x)
        {
            if (x > 127 || x < -128)
            {
                throw new Exception(string.Format("Int8的有限范围为[{0:d},{1:d}]。", -127, 128));
            }

            this.Write((byte) x);
        }

        // Token: 0x0600421F RID: 16927 RVA: 0x0001D8B9 File Offset: 0x0001BAB9
        public void WriteUint8(uint x)
        {
            if (x > 255u)
            {
                throw new Exception(string.Format("Uint8的有限范围为[{0:d},{1:d}]。", 0, 255));
            }

            this.Write((byte) x);
        }

        // Token: 0x06004220 RID: 16928 RVA: 0x0016C6A8 File Offset: 0x0016A8A8
        public void WriteInt16(int x)
        {
            if (x > 32767 || x < -32768)
            {
                throw new Exception(string.Format("int16的有限范围为[{0:d},{1:d}]。", -32768, 32767));
            }

            this.Write((short) x);
        }

        // Token: 0x06004221 RID: 16929 RVA: 0x0001D8EE File Offset: 0x0001BAEE
        public void WriteUint16(uint x)
        {
            if (x > 65535u)
            {
                throw new Exception(string.Format("Uint16的有限范围为[{0:d},{1:d}]。", 0, 65535L));
            }

            this.Write((short) x);
        }

        // Token: 0x06004222 RID: 16930 RVA: 0x0001D924 File Offset: 0x0001BB24
        public void WriteInt32(int x)
        {
            this.Write(x);
        }

        // Token: 0x06004223 RID: 16931 RVA: 0x0001D92D File Offset: 0x0001BB2D
        public void WriteUint32(uint x)
        {
            this.Write(x);
        }

        // Token: 0x06004224 RID: 16932 RVA: 0x0001D936 File Offset: 0x0001BB36
        public void WriteInt64(long x)
        {
            this.Write(x);
        }

        // Token: 0x06004225 RID: 16933 RVA: 0x0001D93F File Offset: 0x0001BB3F
        public void WriteUint64(ulong x)
        {
            this.Write(x);
        }

        // Token: 0x06004226 RID: 16934 RVA: 0x0001D948 File Offset: 0x0001BB48
        public void WriteBool(bool x)
        {
            if (x)
            {
                this.WriteInt8(1);
            }
            else
            {
                this.WriteInt8(0);
            }
        }

        // Token: 0x06004227 RID: 16935 RVA: 0x0016C6F8 File Offset: 0x0016A8F8
        public void WriteFloat(float x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian)
            {
                this.Write(bytes[3]);
                this.Write(bytes[2]);
                this.Write(bytes[1]);
                this.Write(bytes[0]);
            }
            else
            {
                this.Write(bytes[0]);
                this.Write(bytes[1]);
                this.Write(bytes[2]);
                this.Write(bytes[3]);
            }
        }

        // Token: 0x06004228 RID: 16936 RVA: 0x0016C764 File Offset: 0x0016A964
        public void WriteDouble(double x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian)
            {
                this.Write(bytes[7]);
                this.Write(bytes[6]);
                this.Write(bytes[5]);
                this.Write(bytes[4]);
                this.Write(bytes[3]);
                this.Write(bytes[2]);
                this.Write(bytes[1]);
                this.Write(bytes[0]);
            }
            else
            {
                this.Write(bytes[0]);
                this.Write(bytes[1]);
                this.Write(bytes[2]);
                this.Write(bytes[3]);
                this.Write(bytes[4]);
                this.Write(bytes[5]);
                this.Write(bytes[6]);
                this.Write(bytes[7]);
            }
        }

        // Token: 0x06004229 RID: 16937 RVA: 0x0001D963 File Offset: 0x0001BB63
        public void WriteString(string x)
        {
            if (x == null)
            {
                this.WriteBuffer(null);
            }
            else
            {
                this.WriteBuffer(Encoding.UTF8.GetBytes(x));
            }
        }

        // Token: 0x0600422A RID: 16938 RVA: 0x0001D988 File Offset: 0x0001BB88
        public void WriteString2(string x)
        {
            if (x == null)
            {
                this.WriteBuffer2(null);
            }
            else
            {
                this.WriteBuffer2(Encoding.UTF8.GetBytes(x));
            }
        }

        // Token: 0x0600422B RID: 16939 RVA: 0x0001D9AD File Offset: 0x0001BBAD
        public void WriteString4(string x)
        {
            if (x == null)
            {
                this.WriteBuffer4(null);
            }
            else
            {
                this.WriteBuffer4(Encoding.UTF8.GetBytes(x));
            }
        }

        // Token: 0x0600422C RID: 16940 RVA: 0x0001D9D2 File Offset: 0x0001BBD2
        public void WriteBuffer(byte[] bs)
        {
            if (bs == null)
            {
                this.WriteInt8(0);
                return;
            }

            if (bs.Length != 0)
            {
                this.Write(bs, 0, bs.Length);
            }
            else
            {
                this.WriteInt8(1);
            }
        }

        // Token: 0x0600422D RID: 16941 RVA: 0x0016C818 File Offset: 0x0016AA18
        public void WriteBuffer2(byte[] bs)
        {
            if (bs == null)
            {
                this.WriteUint16(0u);
                this.WriteInt8(0);
                return;
            }

            if (bs.Length > 65535)
            {
                throw new Exception("发送字符串失败（字符串长度超过255个字节）。");
            }

            this.WriteUint16((uint) bs.Length);
            if (bs.Length != 0)
            {
                this.Write(bs, 0, bs.Length);
            }
            else
            {
                this.WriteInt8(1);
            }
        }

        // Token: 0x0600422E RID: 16942 RVA: 0x0001DA01 File Offset: 0x0001BC01
        public void WriteBuffer4(byte[] bs)
        {
            if (bs == null)
            {
                this.WriteUint32(0u);
                this.WriteInt8(0);
                return;
            }

            this.WriteUint32((uint) bs.Length);
            if (bs.Length != 0)
            {
                this.Write(bs, 0, bs.Length);
            }
            else
            {
                this.WriteInt8(1);
            }
        }

        // Token: 0x0600422F RID: 16943 RVA: 0x0016C87C File Offset: 0x0016AA7C
        public byte[] ToArray()
        {
            if (this.Length == 0)
            {
                return null;
            }

            byte[] array = new byte[this.Length];
            Array.Copy(this.pktBuffer, 0, array, 0, this.Length);
            this.Seek = 0;
            this.Length = 0;
            return array;
        }

        // Token: 0x06004230 RID: 16944 RVA: 0x0001DA40 File Offset: 0x0001BC40
        public void ToArrayRef(out byte[] _bs, out int _offset, out int _count)
        {
            _bs = this.pktBuffer;
            _offset = 0;
            _count = this.Length;
            this.Seek = 0;
            this.Length = 0;
        }

        // Token: 0x06004231 RID: 16945 RVA: 0x0001DA63 File Offset: 0x0001BC63
        public static int ComputeUint32Size(uint value)
        {
            if ((value & 4294967168u) == 0u)
            {
                return 1;
            }

            if ((value & 4294950912u) == 0u)
            {
                return 2;
            }

            if ((value & 4292870144u) == 0u)
            {
                return 3;
            }

            if ((value & 4026531840u) == 0u)
            {
                return 4;
            }

            return 5;
        }

        // Token: 0x06004232 RID: 16946 RVA: 0x0016C8C8 File Offset: 0x0016AAC8
        public static int computeUint64Size(ulong value)
        {
            if ((value & 18446744073709551488UL) == 0UL)
            {
                return 1;
            }

            if ((value & 18446744073709535232UL) == 0UL)
            {
                return 2;
            }

            if ((value & 18446744073707454464UL) == 0UL)
            {
                return 3;
            }

            if ((value & 18446744073441116160UL) == 0UL)
            {
                return 4;
            }

            if ((value & 18446744039349813248UL) == 0UL)
            {
                return 5;
            }

            if ((value & 18446739675663040512UL) == 0UL)
            {
                return 6;
            }

            if ((value & 18446181123756130304UL) == 0UL)
            {
                return 7;
            }

            if ((value & 18374686479671623680UL) == 0UL)
            {
                return 8;
            }

            if ((value & 9223372036854775808UL) == 0UL)
            {
                return 9;
            }

            return 10;
        }

        // Token: 0x04002F6B RID: 12139
        private byte[] pktBuffer;
    }
}
