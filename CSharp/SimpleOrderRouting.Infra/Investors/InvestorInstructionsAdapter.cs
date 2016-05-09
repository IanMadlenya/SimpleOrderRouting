﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvestorInstructionsAdapter.cs" company="LunchBox corp">
//     Copyright 2014 The Lunch-Box mob: 
//           Ozgur DEVELIOGLU (@Zgurrr)
//           Cyrille  DUPUYDAUBY (@Cyrdup)
//           Tomasz JASKULA (@tjaskula)
//           Mendel MONTEIRO-BECKERMAN (@MendelMonteiro)
//           Thomas PIERRAIN (@tpierrain)
//     
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//         http://www.apache.org/licenses/LICENSE-2.0
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SimpleOrderRouting.Infra
{
    using System;
    using System.Collections.Generic;

    using SimpleOrderRouting.Investors;
    using SimpleOrderRouting.Markets.Orders;

    //TODO: add tests for this adapter

    /// <summary>
    /// External API for the Smart Order Routing service. Aggregates all instruction events.
    /// (Hexagonal) Adapter from the infrastructure code to the domain code and vice-versa.
    /// </summary>
    public class InvestorInstructionsAdapter
    {
        private readonly IHandleInvestorInstructions sor;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestorInstructionsAdapter"/> class.
        /// </summary>
        /// <param name="sor">The sor.</param>
        public InvestorInstructionsAdapter(IHandleInvestorInstructions sor)
        {
            this.sor = sor;
        }

        public event EventHandler<InvestorInstructionUpdatedDto> InstructionUpdated;

        public void Route(InvestorInstructionDto investorInstructionDto)
        {
            // Adapt from infra to domain
            var investorInstruction = this.CreateInvestorInstruction(investorInstructionDto.UniqueIdentifier, null, investorInstructionDto.Way, investorInstructionDto.Quantity, investorInstructionDto.Price, investorInstructionDto.AllowPartialExecution, investorInstructionDto.GoodTill);
            this.sor.Route(investorInstruction);
        }

        private InvestorInstruction CreateInvestorInstruction(InvestorInstructionIdentifierDto instructionIdentifierDto, InstrumentIdentifier instrumentIdentifier, Way way, int quantity, decimal price, bool allowPartialExecution = false, DateTime? goodTill = null)
        {
            return new InvestorInstruction(instructionIdentifierDto.Value, way, quantity, price, allowPartialExecution, goodTill);
        }

        // TODO: expose some infra callbacks instead of the domain one
        public void Subscribe(InvestorInstructionDto investorInstructionDto, Action<OrderExecutedEventArgs> executedCallback, Action<string> failureCallback)
        {
            // Maps the DTO model to the domain one
            var investorIntruction = new InvestorInstruction(investorInstructionDto.UniqueIdentifier.Value, investorInstructionDto.Way, investorInstructionDto.Quantity, investorInstructionDto.Price, investorInstructionDto.AllowPartialExecution, investorInstructionDto.GoodTill);

            //investorIntruction.Executed += investorIntruction_Executed;
            //investorIntruction.Failed += investorIntruction_Failed;

            //// TODO: regirst failure
            var dtoCallBacks = new InvestorInstructionDtoCallBacks(executedCallback, failureCallback);
            this.sor.Subscribe(investorIntruction, dtoCallBacks.ExecutedCallback, dtoCallBacks.FailedCallbacks);
            
            // TODO: cleanup the dtoCallback resource
        }
    }
}