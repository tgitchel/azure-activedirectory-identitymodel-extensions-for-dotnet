﻿//-----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

// This file contains derived types that are usefull across multiple handlers / protocols.


using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;

namespace System.IdentityModel.Tokens.Tests
{
#if DNXCORE50
    public class DerivedClaim : Claim
    {
        string _dataString;
        byte[] _dataBytes;

        public DerivedClaim(Claim claim, string dataString, byte[] dataBytes)
            : base(claim)
        {
            _dataString = dataString;
            _dataBytes = dataBytes.CloneByteArray();
        }

        public DerivedClaim(DerivedClaim other)
            : this(other, (ClaimsIdentity)null)
        { }

        public DerivedClaim(DerivedClaim other, ClaimsIdentity subject)
            : base(other, subject)
        {
            _dataString = other._dataString;
            if (other._dataBytes != null)
                _dataBytes = other._dataBytes.CloneByteArray();
        }

        public DerivedClaim(BinaryReader reader)
            : this(reader, (ClaimsIdentity)null)
        { }

        public DerivedClaim(BinaryReader reader, ClaimsIdentity subject)
            : base(reader, subject)
        {
            _dataString = reader.ReadString();
            Int32 cb = reader.ReadInt32();
            if (cb > 0)
                _dataBytes = reader.ReadBytes(cb);
        }

        public byte[] DataBytes
        {
            get
            {
                return _dataBytes;
            }

            set
            {
                _dataBytes = value;
            }
        }

        public string DataString
        {
            get
            {
                return _dataString;
            }

            set
            {
                _dataString = value;
            }
        }

        public override Claim Clone()
        {
            return Clone((ClaimsIdentity)null);
        }

        public override Claim Clone(ClaimsIdentity identity)
        {
            return new DerivedClaim(this, identity);
        }

        public override void WriteTo(IO.BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_dataString);
            if (_dataBytes == null || _dataBytes.Length == 0)
            {
                writer.Write((Int32)0);
            }
            else
            {
                writer.Write((Int32)_dataBytes.Length);
                writer.Write(_dataBytes);
            }
        }
    }

    public class DerivedClaimsIdentity : ClaimsIdentity
    {
        string _dataString;
        byte[] _dataBytes;

        public DerivedClaimsIdentity(BinaryReader reader)
            : base(reader)
        {
            _dataString = reader.ReadString();
            Int32 cb = reader.ReadInt32();
            if (cb > 0)
                _dataBytes = reader.ReadBytes(cb);

        }

        public DerivedClaimsIdentity(IEnumerable<Claim> claims, string dataString, byte[] dataBytes)
            : base(claims)
        {
            _dataString = dataString;

            if (dataBytes != null && dataBytes.Length > 0)
                _dataBytes = dataBytes.CloneByteArray();
        }

        public string ClaimType { get; set; }

        public byte[] DataBytes
        {
            get
            {
                return _dataBytes;
            }

            set
            {
                _dataBytes = value;
            }
        }

        public string DataString
        {
            get
            {
                return _dataString;
            }

            set
            {
                _dataString = value;
            }
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_dataString);
            if (_dataBytes == null || _dataBytes.Length == 0)
            {
                writer.Write((Int32)0);
            }
            else
            {
                writer.Write((Int32)_dataBytes.Length);
                writer.Write(_dataBytes);
            }

            writer.Flush();
        }

        protected override Claim CreateClaim(BinaryReader reader)
        {
            return new DerivedClaim(reader, this);
        }
    }

    public class DerivedClaimsPrincipal : ClaimsPrincipal
    {
    }
#else
    public class DerivedClaim : Claim
    {
        public DerivedClaim(Claim claim, string data, byte[] bytes)
            : base(claim.Value, claim.Type)
        {
        }
    }

    public class DerivedClaimsIdentity : ClaimsIdentity
    {
        public DerivedClaimsIdentity(IEnumerable<Claim> claims, string data, byte[] bytes)
            : base(claims)
        {

        }
    }

    public class DerivedClaimsPrincipal : ClaimsPrincipal
    {

    }
#endif
    public class NotAsymmetricOrSymmetricSecurityKey : SecurityKey
    {
        public override SignatureProvider GetSignatureProvider(string algorithm, bool verifyOnly)
        {
            throw new NotImplementedException();
        }

        public override int KeySize
        {
            get { throw new NotImplementedException(); }
        }

        public static SecurityKey New { get { return new NotAsymmetricOrSymmetricSecurityKey(); } }
    }

    public class ReturnNullSymmetricSecurityKey : SymmetricSecurityKey
    {
        public ReturnNullSymmetricSecurityKey(byte[] keyBytes)
            : base(keyBytes)
        { }

        public SymmetricSecurityKey SymmetricSecurityKey { get; set; }

        public override byte[] Key
        {
            get
            {
                if (SymmetricSecurityKey == null)
                {
                    return null;
                }

                return SymmetricSecurityKey.Key;
            }
        }

        public override int KeySize
        {
            get
            {
                if (SymmetricSecurityKey != null)
                {
                    return SymmetricSecurityKey.KeySize;
                }

                return 256;
            }
        }
    }

    public class ReturnNullAsymmetricSecurityKey : AsymmetricSecurityKey
    {
        public ReturnNullAsymmetricSecurityKey() { }

        public override bool HasPrivateKey
        {
            get { throw new NotImplementedException(); }
        }

        public override SignatureProvider GetSignatureProvider(string algorithm, bool verifyOnly)
        {
            throw new NotImplementedException();
        }

        public override int KeySize
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasPublicKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Useful for trigging an exception.
    /// </summary>
    public class FaultingAsymmetricSecurityKey : AsymmetricSecurityKey
    {
        AsymmetricSecurityKey _key;

        public FaultingAsymmetricSecurityKey(AsymmetricSecurityKey key = null, AsymmetricAlgorithm agorithm = null, bool hasPrivateKey = false)
        {
            _key = key;
        }

        public override bool HasPrivateKey
        {
            get { return _key.HasPrivateKey; }
        }

        public override bool HasPublicKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int KeySize { get { return _key.KeySize; } }

        public override SignatureProvider GetSignatureProvider(string algorithm, bool verifyOnly)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// This works that is a parameter is null, we throw the exception when asked for the property
    /// </summary>
    public class FaultingSymmetricSecurityKey : SymmetricSecurityKey
    {
        Exception _throwMe;
        KeyedHashAlgorithm _keyedHash;
        SymmetricSecurityKey _key;
        SymmetricAlgorithm _agorithm;
        byte[] _keyBytes;

        public FaultingSymmetricSecurityKey(SymmetricSecurityKey key, Exception throwMe, SymmetricAlgorithm algorithm = null, KeyedHashAlgorithm keyedHash = null, byte[] keyBytes = null)
            : base(keyBytes)
        {
            _throwMe = throwMe;
            _key = key;
            _keyedHash = keyedHash;
            _agorithm = algorithm;
            _keyBytes = keyBytes;
        }

        public override byte[] Key
        {
            get
            {
                if (_throwMe != null)
                    throw _throwMe;

                return _keyBytes;
            }
        }

        public override int KeySize { get { return _key.KeySize; } }
    }

    public class FaultingKeyedHashAlgorithm : KeyedHashAlgorithm
    {
        KeyedHashAlgorithm _keyedHashAlgorithm;
        Exception _throwMe;
        byte[] _key;

        public FaultingKeyedHashAlgorithm(KeyedHashAlgorithm keyedHashAlgorithm, Exception throwMe, byte[] key)
        {
            _keyedHashAlgorithm = keyedHashAlgorithm;
            _throwMe = throwMe;
            _key = key;
        }

        public override byte[] Key
        {
            get
            {
                if (_throwMe != null)
                {
                    throw _throwMe;
                }

                return _key;
            }

            set
            {
                _key = value;
            }
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            throw new NotImplementedException();
        }

        protected override byte[] HashFinal()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Helpful for extensibility testing for errors.
    /// </summary>
    public class AlwaysReturnNullSignatureProviderFactory : SignatureProviderFactory
    {
        public override SignatureProvider CreateForSigning(SecurityKey key, string algorithm)
        {
            return null;
        }

        public override SignatureProvider CreateForVerifying(SecurityKey key, string algorithm)
        {
            return null;
        }
    }

}