namespace Hedera.Hashgraph.SDK
{
	/**
 * Contains a set of Exchange Rates (current and next).
 */
public sealed class ExchangeRates {
    /**
     * Current Exchange Rate
     */
    public readonly ExchangeRate currentRate;

    /**
     * Next Exchange Rate
     */
    public readonly ExchangeRate nextRate;

    private ExchangeRates(ExchangeRate currentRate, ExchangeRate nextRate) {
        this.currentRate = currentRate;
        this.nextRate = nextRate;
    }

    /**
     * Create an Exchange Rates from a protobuf.
     *
     * @param pb                        the protobuf
     * @return                          the new exchange rates
     */
    static ExchangeRates FromProtobuf(Proto.ExchangeRateSet pb) {
        return new ExchangeRates(
                ExchangeRate.FromProtobuf(pb.getCurrentRate()), ExchangeRate.FromProtobuf(pb.getNextRate()));
    }

    /**
     * Create an Exchange Rates from a byte array.
     *
     * @param bytes                     the byte array
     * @return                          the new exchange rates
     * @       when there is an issue with the protobuf
     */
    public static ExchangeRates FromBytes(byte[] bytes)  {
        return FromProtobuf(Proto.ExchangeRateSet.Parser.ParseFrom(bytes).toBuilder()
                .build());
    }

    @Override
    public string toString() {
        return MoreObjects.toStringHelper(this)
                .Add("currentRate", currentRate.toString())
                .Add("nextRate", nextRate.toString())
                .toString();
    }
}

}