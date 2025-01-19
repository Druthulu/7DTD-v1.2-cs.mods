using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS
{
	public class AuthClient : IAuthenticationClient
	{
		public ConnectInterface connectInterface
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((Api)this.owner.Api).ConnectInterface;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		public string GetAuthTicket()
		{
			EosHelpers.AssertMainThread("ACl.Get");
			CopyIdTokenOptions copyIdTokenOptions = new CopyIdTokenOptions
			{
				LocalUserId = ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId
			};
			object lockObject = AntiCheatCommon.LockObject;
			IdToken? idToken;
			Result result;
			lock (lockObject)
			{
				result = this.connectInterface.CopyIdToken(ref copyIdTokenOptions, out idToken);
			}
			Log.Out(string.Format("[EOS] CopyIdToken result: {0}", result));
			return (idToken != null) ? idToken.GetValueOrDefault().JsonWebToken : null;
		}

		public void AuthenticateServer(ClientAuthenticateServerContext _context)
		{
			AuthClient.<>c__DisplayClass5_0 CS$<>8__locals1 = new AuthClient.<>c__DisplayClass5_0();
			CS$<>8__locals1._context = _context;
			EosHelpers.AssertMainThread("ACl.Auth");
			if (PermissionsManager.IsCrossplayAllowed())
			{
				CS$<>8__locals1._context.Success();
				return;
			}
			if (CS$<>8__locals1._context.GameServerInfo.AllowsCrossplay)
			{
				Log.Error("[EOS] [ACl.Auth] Cannot join server that has crossplay when we do not have crossplay permissions.");
				CS$<>8__locals1._context.DisconnectNoCrossplay();
				return;
			}
			if (EPlayGroupExtensions.Current == EPlayGroup.Standalone && (CS$<>8__locals1._context.GameServerInfo.PlayGroup == EPlayGroup.Standalone || CS$<>8__locals1._context.GameServerInfo.IsDedicated))
			{
				CS$<>8__locals1._context.Success();
				return;
			}
			PlatformUserIdentifierAbs crossplatformUserId = CS$<>8__locals1._context.CrossplatformUserId;
			CS$<>8__locals1.identifierEos = (crossplatformUserId as UserIdentifierEos);
			if (CS$<>8__locals1.identifierEos == null)
			{
				Log.Warning(string.Format("[EOS] [ACl.Auth] Expected EOS Crossplatform ID? But got: {0}", CS$<>8__locals1._context.CrossplatformUserId));
				CS$<>8__locals1._context.DisconnectNoCrossplay();
				return;
			}
			IdToken value = new IdToken
			{
				JsonWebToken = CS$<>8__locals1.identifierEos.Ticket,
				ProductUserId = CS$<>8__locals1.identifierEos.ProductUserId
			};
			VerifyIdTokenOptions verifyIdTokenOptions = new VerifyIdTokenOptions
			{
				IdToken = new IdToken?(value)
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.connectInterface.VerifyIdToken(ref verifyIdTokenOptions, null, new OnVerifyIdTokenCallback(CS$<>8__locals1.<AuthenticateServer>g__VerifyIdTokenCallback|0));
			}
		}

		public void Destroy()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;
	}
}
