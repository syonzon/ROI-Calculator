import React, { useState } from 'react';
import './App.css';

function App() {
  // ROI Calculator State
  const [investment, setInvestment] = useState(0);
  const [gains, setGains] = useState(0);
  const [roi, setRoi] = useState('');

  // NPV Calculator State
  const [cashFlows, setCashFlows] = useState([]);
  const [discountRate, setDiscountRate] = useState(0);
  const [npv, setNpv] = useState('');

  // Payback Period Calculator State
  const [initialInvestment, setInitialInvestment] = useState(0);
  const [annualCashFlow, setAnnualCashFlow] = useState(0);
  const [paybackPeriod, setPaybackPeriod] = useState('');

  // ROI Calculation
  const calculateROI = async () => {
    try {
      const response = await fetch(
        `https://roi-calculator-function.azurewebsites.net/api/HttpTrigger1?investment=${investment}&gains=${gains}`
      );
      const result = await response.text();
      setRoi(result);
    } catch (error) {
      console.error('Error calculating ROI:', error);
      setRoi('Error calculating ROI. Please try again.');
    }
  };

  // NPV Calculation
  const calculateNPV = () => {
    let npvValue = 0;
    for (let t = 0; t < cashFlows.length; t++) {
      npvValue += cashFlows[t] / Math.pow(1 + discountRate, t + 1);
    }
    setNpv(`NPV: ${npvValue.toFixed(2)}`);
  };

  // Payback Period Calculation
  const calculatePaybackPeriod = () => {
    if (annualCashFlow === 0) {
      setPaybackPeriod('Annual cash flow cannot be zero.');
      return;
    }
    const period = initialInvestment / annualCashFlow;
    setPaybackPeriod(`Payback Period: ${period.toFixed(2)} years`);
  };

  return (
    <div className="App">
      <h1>ROI Calculator</h1>
      <div>
        <label>Total Investment ($): </label>
        <input
          type="number"
          value={investment}
          onChange={(e) => setInvestment(parseFloat(e.target.value))}
        />
      </div>
      <div>
        <label>Total Gains ($): </label>
        <input
          type="number"
          value={gains}
          onChange={(e) => setGains(parseFloat(e.target.value))}
        />
      </div>
      <button onClick={calculateROI}>Calculate ROI</button>
      <h2>{roi}</h2>

      <h1>NPV Calculator</h1>
      <div>
        <label>Discount Rate (%): </label>
        <input
          type="number"
          value={discountRate}
          onChange={(e) => setDiscountRate(parseFloat(e.target.value))}
        />
      </div>
      <div>
        <label>Cash Flows ($): </label>
        <input
          type="text"
          placeholder="Enter comma-separated values"
          onChange={(e) => setCashFlows(e.target.value.split(',').map(Number))}
        />
      </div>
      <button onClick={calculateNPV}>Calculate NPV</button>
      <h2>{npv}</h2>

      <h1>Payback Period Calculator</h1>
      <div>
        <label>Initial Investment ($): </label>
        <input
          type="number"
          value={initialInvestment}
          onChange={(e) => setInitialInvestment(parseFloat(e.target.value))}
        />
      </div>
      <div>
        <label>Annual Cash Flow ($): </label>
        <input
          type="number"
          value={annualCashFlow}
          onChange={(e) => setAnnualCashFlow(parseFloat(e.target.value))}
        />
      </div>
      <button onClick={calculatePaybackPeriod}>Calculate Payback Period</button>
      <h2>{paybackPeriod}</h2>
    </div>
  );
}

export default App;