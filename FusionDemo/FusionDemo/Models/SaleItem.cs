using DataMeshGroup.Fusion.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FusionDemo.Models
{
    public class SaleItem : DataMeshGroup.Fusion.Model.SaleItem
    {
        public string AllTags 
        { 
            get
            {
                string allTags = string.Empty;
                if (Tags != null){
                    allTags = string.Join(",", Tags);
                }
                return allTags;
            }
        }

    }
}
