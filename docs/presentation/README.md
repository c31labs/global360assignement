# Presentation

Two formats are provided so the reviewer can pick whichever is easiest.

## Public link (HTML deck)

Once GitHub Pages is enabled on this repo (Settings → Pages → Source: `main`, Folder: `/docs`), the slides are available at:

**<https://c31labs.github.io/global360assignement/presentation/>**

If Pages isn't enabled yet, you can view the deck locally:

```bash
cd docs/presentation
python3 -m http.server 8000
# open http://localhost:8000/
```

## PowerPoint

The same deck is exported as [`TaskFlow.pptx`](TaskFlow.pptx) in this folder. Use whichever fits your workflow.

## Contents

1. **Solution architecture.** Production diagram, technology choices with trade-offs, NFRs (security, scalability, reliability, observability), assumptions.
2. **Delivery plan.** Phased roadmap, team composition, top risks with mitigations, progress tracking.
3. **Technical leadership.** Ways of working, handling a PO pushing risky scope, day one questions about the brief.
