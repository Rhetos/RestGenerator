﻿/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Rhetos.Persistence;

namespace Rhetos.JsonCommands.Host.Filters
{
    /// <summary>
    /// Automatically commit unit of work on successful response with code 200, rollback otherwise.
    /// </summary>
    public class ApiCommitOnSuccessFilter : IActionFilter, IOrderedFilter
    {
        private readonly IRhetosComponent<IUnitOfWork> rhetosUnitOfWork;
        public int Order { get; } = int.MaxValue - 20;

        public ApiCommitOnSuccessFilter(IRhetosComponent<IUnitOfWork> rhetosUnitOfWork)
        {
            this.rhetosUnitOfWork = rhetosUnitOfWork;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Response.StatusCode == 200 && context.Exception == null)
                rhetosUnitOfWork.Value.CommitAndClose();
            else
                rhetosUnitOfWork.Value.RollbackAndClose();
        }
    }
}
