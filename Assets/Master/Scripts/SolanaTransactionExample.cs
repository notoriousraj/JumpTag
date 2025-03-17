using System;
using System.Threading.Tasks;
using Solana.Unity.Rpc;
using Solana.Unity.Wallet;
using Solana.Unity.SDK;
using UnityEngine;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Models;
using System.Collections.Generic;

public class SolanaTransactionExample : MonoBehaviour
{
    private static readonly string EscrowAccount = "CAT8oVWLhsu8m7498bEiMTk6az8p6qNHMBbpcGw7wpHj";
    private static readonly ulong EntryFeeLamports = 0;//000_000; // 0.01 SOL

    public async void StartTrans()
    {
        bool success = await TransferEntryFeeToEscrow();
        Debug.Log(success ? "✅ Transaction completed successfully!" : "❌ Transaction failed.");
    }

    public async Task<bool> TransferEntryFeeToEscrow()
    {
        if (Web3.Wallet == null || Web3.Wallet.Account == null)
        {
            Debug.LogError("❌ Wallet not connected!");
            return false;
        }

        var userAccount = Web3.Wallet.Account;
        var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
        var escrowPublicKey = new PublicKey(EscrowAccount);

        // 🔹 Check balance
        var balanceResult = await rpcClient.GetBalanceAsync(userAccount.PublicKey);
        if (!balanceResult.WasSuccessful)
        {
            Debug.LogError("❌ Failed to fetch balance: " + balanceResult.Reason);
            return false;
        }

        ulong balance = balanceResult.Result.Value;
        Debug.Log($"🔵 Wallet Balance: {balance / 1_000_000_000.0} SOL");

        if (balance < EntryFeeLamports)
        {
            Debug.LogError("❌ Insufficient SOL balance to pay the entry fee.");
            return false;
        }

        // 🔹 Get latest blockhash
        var blockHashResult = await rpcClient.GetLatestBlockHashAsync();
        if (!blockHashResult.WasSuccessful)
        {
            Debug.LogError("❌ Failed to fetch blockhash: " + blockHashResult.Reason);
            return false;
        }

        string recentBlockhash = blockHashResult.Result.Value.Blockhash;
        Debug.Log("🟢 Recent Blockhash: " + recentBlockhash);

        // 🔹 Verify escrow account exists
        var escrowInfo = await rpcClient.GetAccountInfoAsync(escrowPublicKey);
        if (escrowInfo.Result.Value == null)
        {
            Debug.LogError("❌ Escrow account does NOT exist. Ensure it has been created!");
            return false;
        }

        Debug.Log($"✅ Escrow account found, owner: {escrowInfo.Result.Value.Owner}");

        // 🔹 Create transfer transaction
        var transaction = new TransactionBuilder()
            .SetRecentBlockHash(recentBlockhash)
            .SetFeePayer(userAccount.PublicKey)
            .AddInstruction(SystemProgram.Transfer(
                userAccount.PublicKey,
                escrowPublicKey,
                EntryFeeLamports));

        // 🔹 Sign & Serialize the transaction
        byte[] signedTransaction = transaction.Build(new List<Account> { userAccount });
        string serializedTx = Convert.ToBase64String(signedTransaction);

        Debug.Log("🔵 Signed Transaction (Base64): " + serializedTx);

        // 🔹 Send transaction
        var sendResult = await rpcClient.SendTransactionAsync(serializedTx);
        if (sendResult.WasSuccessful)
        {
            Debug.Log($"✅ Transaction successful! Tx ID: {sendResult.Result}");
            return true;
        }
        else
        {
            Debug.LogError($"❌ Transaction failed: {sendResult.Reason}");
            return false;
        }
    }
}
