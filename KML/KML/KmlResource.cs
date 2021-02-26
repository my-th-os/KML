﻿using System.Globalization;

namespace KML
{
    /// <summary>
    /// KmlResource represents a KmlNode with the "RESOURCE" tag.
    /// </summary>
    public class KmlResource : KmlNode
    {
        /// <summary>
        /// Get the "amount" attribute as property.
        /// </summary>
        public KmlAttrib Amount { get; private set; }

        /// <summary>
        /// Get the "maxAmount" attribute as property.
        /// </summary>
        public KmlAttrib MaxAmount { get; private set; }

        /// <summary>
        /// Get the ratio of Amout / MaxAmout as double.
        /// </summary>
        public double AmountRatio
        {
            get
            {
                double maxAmount = 0.0;
                double.TryParse(MaxAmount.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out maxAmount);
                double amount = 0.0;
                double.TryParse(Amount.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);

                if (maxAmount == 0.0)
                {
                    // avoid dividing by zero
                    maxAmount = amount = 1.0;
                }
                return amount / maxAmount;
            }
        }

        /// <summary>
        /// Creates a KmlResource as a copy of a given KmlNode.
        /// </summary>
        /// <param name="node">The KmlNode to copy</param>
        public KmlResource(KmlNode node)
            : base(node.Line)
        {
            Amount = new KmlAttrib("amount =");
            MaxAmount = new KmlAttrib("maxAmount =");

            AddRange(node.AllItems);
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode, KmlPart, KmlAttrib or further derived from these.
        /// When an KmlAttrib "Name", "Amount" or "MaxAmount" are found, they 
        /// will be used for the corresponding property of this node.
        /// </summary>
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="newItem">The KmlItem to add</param>
        protected override void Add(KmlItem beforeItem, KmlItem newItem)
        {
            if (newItem is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)newItem;
                if (attrib.Name.ToLower() == "amount")
                {
                    Amount = attrib;

                    // Get notified when Amount changes
                    attrib.AttribValueChanged += Amount_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "maxamount")
                {
                    MaxAmount = attrib;

                    // Get notified when MaxAmount changes
                    attrib.AttribValueChanged += MaxAmount_Changed;
                    attrib.CanBeDeleted = false;
                }
            }
            base.Add(beforeItem, newItem);
        }

        /// <summary>
        /// Clear all child nodes and attributes from this node.
        /// </summary>
        public override void Clear()
        {
            Amount.Value = "";
            MaxAmount.Value = "";
            base.Clear();
        }

        /// <summary>
        /// Refill this resource by setting "amount" value to "maxAmount" value.
        /// </summary>
        public void Refill()
        {
            Amount.Value = MaxAmount.Value;
        }

        private void MaxAmount_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            InvokeToStringChanged();
        }

        private void Amount_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            InvokeToStringChanged();
        }
    }
}
