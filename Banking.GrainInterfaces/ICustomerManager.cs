﻿using Banking.GrainInterfaces.Commands;

namespace Banking.GrainInterfaces
{
    public interface ICustomerManager : IGrainWithStringKey
    {
        Task CreateCustomer(CreateCustomerCommand command);
        Task UpdatePrimaryAccountHolder(UpdatePrimaryAccountHolderCommand command);
        Task UpdatePrimaryResidence(UpdatePrimaryResidenceCommand command);
        Task UpdateSpouse(UpdateSpouseCommand command);
        Task UpdateSpouseResidence(UpdateSpouseyResidenceCommand command);
        Task RemoveSpouse(RemoveSpouseCommand command);
        Task UpdateMailingAddress(UpdateMailingAddressCommand command);
        Task AddAccount(AddAccountCommand command);
        Task RemoveAccount(RemoveAccountCommand command);
        Task PostTransaction(PostTransactionCommand command);
    }
}