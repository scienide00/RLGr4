﻿//
//  IOrgan.cs
//
//  Author:
//       scienide <alexandar921@abv.bg>
//
//  Copyright (c) 2015 scienide
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace RLG.Framework.Anatomy.Contracts
{
    using System.Collections.Generic;
    using RLG.Contracts;

    public interface IOrgan
    {
        IPropertyBag<string> Properties { get; set; }

        List<IOrgan> ParentOrgans { get; set; }

        List<IOrgan> ChildOrgans { get; set; }
    }
}