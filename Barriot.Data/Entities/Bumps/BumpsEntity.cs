using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using Barriot.Entities.Bumps;

namespace Barriot
{
    [BsonIgnoreExtraElements]
    public class BumpsEntity : IMutableEntity, IConcurrentlyAccessible<BumpsEntity>
    {
        /// <inheritdoc/>
        [BsonId]
        public ObjectId ObjectId { get; set; }

        /// <inheritdoc/>
        [BsonIgnore]
        public EntityState State { get; set; } = EntityState.Deserializing;

        public BumpsEntity(ulong userId)
        {
            _receivedBumps = new();
            _bumpsToGive = 0;
            UserId = userId;
        }

        #region BumpsEntity

        /// <summary>
        ///     The user ID of this entity.
        /// </summary>
        public ulong UserId { get; set; }

        private long _bumpsToGive;
        /// <summary>
        ///     All bumps this user can grant.
        /// </summary>
        public long BumpsToGive
        {
            get
                => _bumpsToGive;
            set
            {
                _ = ModifyAsync(Builders<BumpsEntity>.Update.Set(x => x.BumpsToGive, value));
                _bumpsToGive = value;
            }
        }

        private DateTime _lastRedeemed;
        /// <summary>
        ///     Sets when a /daily has been redeemed.
        /// </summary>
        public DateTime LastRedeemed
        {
            get
                => _lastRedeemed;
            set
            {
                _ = ModifyAsync(Builders<BumpsEntity>.Update.Set(x => x.LastRedeemed, value));
                _lastRedeemed = value;
            }
        }

        private long _receivedBumps;
        /// <summary>
        ///     All bumps this user has received.
        /// </summary>
        public long ReceivedBumps
        {
            get
                => _receivedBumps;
            set
            {
                _ = ModifyAsync(Builders<BumpsEntity>.Update.Set(x => x.ReceivedBumps, value));
                _receivedBumps = value;
            }
        }

        public bool CanRedeem()
        {
            if (LastRedeemed <= DateTime.UtcNow.AddDays(-1))
                return true;
            return false;
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync()
            => BumpsHelper.DeleteAsync(this);

        /// <inheritdoc />
        public Task<bool> ModifyAsync(UpdateDefinition<BumpsEntity> update)
            => BumpsHelper.ModifyAsync(this, update);

        public static async Task<BumpsEntity> GetAsync(ulong userId)
            => await BumpsHelper.GetAsync(userId);

        #endregion

        #region IDisposable
        void IDisposable.Dispose() { }
        #endregion

        #region IMutableEntity
        Task<bool> IMutableEntity.UpdateAsync()
            => throw new NotSupportedException();
        #endregion

        /// <summary>
        ///     Returns the mention of this bump's target user.
        /// </summary>
        /// <returns>A Discord user mention.</returns>
        public override string ToString()
            => $"<@{UserId}>";
    }
}
