using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using SunsetSystems.Economy.UI;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Economy
{
    [Serializable]
    public class CashTradeOffer : AbstractTradeOffer
    {
        [SerializeField, ReadOnly, TableColumnWidth(120, false)]
        private TradeOfferType _offerType;
        [LabelWidth(120)]
        [SerializeField, VerticalGroup("Price Settings")]
        private bool _overrideCost = false;
        [LabelWidth(120)]
        [SerializeField, LabelText("Unit Cost"), ShowIf("@this._overrideCost"), VerticalGroup("Price Settings")]
        private float _costOverrideValue = 10;
        [LabelWidth(120)]
        [ShowInInspector, ReadOnly, PropertyOrder(0), ShowIf("@this._overrideCost == false"), VerticalGroup("Price Settings")]
        private float UnitCost => _overrideCost ? _costOverrideValue : _offerItem?.GetBaseValue() ?? 0f;
        [LabelWidth(120)]
        [SerializeField, MinValue(0), PropertyOrder(1), VerticalGroup("Price Settings")]
        private int _unitAmount = 1;
        [LabelWidth(120)]
        [ShowInInspector, ReadOnly, PropertyOrder(1), VerticalGroup("Price Settings")]
        private float MerchantMarkup => GetMerchantMarkup();
        [LabelWidth(120)]
        [ShowInInspector, ReadOnly, PropertyOrder(2), VerticalGroup("Price Settings")]
        private float TotalOfferCost => CalculateCost();
        [OdinSerialize, PropertyOrder(3), TableColumnWidth(180, false)]
        private ITradeable _offerItem;

        public CashTradeOffer() : base(null)
        {

        }

        public CashTradeOffer(ITradeable offerItem, TradeOfferType offerType, IMerchant owner) : base(owner)
        {
            _offerItem = offerItem;
            _offerType = offerType;
            _unitAmount = 1;
        }

        public override bool AcceptOffer()
        {
            bool result = false;
            switch (_offerType)
            {
                case TradeOfferType.BuyFromPlayer:
                    result = _offerItem.HandlePlayerSold(_unitAmount);
                    break;
                case TradeOfferType.SellToPlayer:
                    result = _offerItem.HandlePlayerBought(_unitAmount);
                    break;
            }
            if (result)
                ITradeOffer.OnTradeOfferAccepted?.Invoke(this);
            return result;
        }

        private float GetMerchantMarkup()
        {
            if (OwnerConfig == null)
                return 0;
            return _offerType switch
            {
                TradeOfferType.BuyFromPlayer => OwnerConfig.GetMerchantBuyMarkup(),
                TradeOfferType.SellToPlayer => OwnerConfig.GetMerchantSellMarkup(),
                _ => 0f,
            };
        }

        private float CalculateCost() => UnitCost * MerchantMarkup * _unitAmount;

        public override float GetCost()
        {
            return TotalOfferCost;
        }

        public override bool CanPlayerAffordOffer()
        {
            return InventoryManager.Instance.GetMoneyAmount() >= GetCost();
        }

        public TradeOfferType GetOfferType() => _offerType;

        public override ITradeOfferViewData GetOfferViewData()
        {
            return new CashOfferViewData(this);
        }

        public class CashOfferViewData : ITradeOfferViewData
        {
            private readonly CashTradeOffer _dataSource;

            public string Title => _dataSource._offerItem.GetTradeName();
            public Sprite Icon => _dataSource._offerItem.GetTradeIcon();
            public int UnitAmount => _dataSource._unitAmount;
            public float OfferPrice => _dataSource.GetCost();

            public Func<bool> OfferAcceptanceDelegate => _dataSource.AcceptOffer;

            public CashOfferViewData(CashTradeOffer dataSource)
            {
                _dataSource = dataSource;
            }

            public bool CanPlayerAffordOffer()
            {
                return _dataSource.CanPlayerAffordOffer();
            }
        }
    }
}