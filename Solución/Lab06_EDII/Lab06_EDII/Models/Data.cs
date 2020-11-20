namespace Lab06_EDII.Models
{
    public class Data
    {
        public bool ValidacionPrimo(int valor, int divisor)
        {
            if (valor / 2 < divisor)
            {
                return true;
            }
            else
            {
                if (valor % divisor == 0)
                {
                    return false;
                }
                else
                {
                    return ValidacionPrimo(valor, divisor + 1);
                }
            }
        }
    }
}